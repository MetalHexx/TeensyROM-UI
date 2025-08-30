using System.Windows.Documents;
using Microsoft.Xaml.Behaviors;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq;

namespace TeensyRom.Ui.Features.Terminal
{
    public class RichTextBindingBehavior : Behavior<RichTextBox>
    {
        public static readonly DependencyProperty TextCollectionProperty =
            DependencyProperty.Register(
                "TextCollection",
                typeof(ObservableCollection<string>),
                typeof(RichTextBindingBehavior),
                new PropertyMetadata(null, OnTextCollectionChanged));

        private readonly DispatcherTimer _updateTimer;
        private readonly DispatcherTimer _trimTimer; // Additional timer for continuous trimming
        private bool _needsUpdate = false;
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(100); // Faster updates
        private Queue<string> _pendingTexts = new();
        private const int MaxItemsPerUpdate = 10; // Reduced batch size (down from 20)
        private bool _initialLoadCompleted = false;
        private int _loadedLogCount = 0;
        
        // Track high volume operations
        private int _blockCountLastCheck = 0;
        private DateTime _lastTrimCheck = DateTime.MinValue;
        private bool _inHighVolumeMode = false;

        // FlowDocument caching
        private static WeakReference<FlowDocument> _cachedDocumentRef;
        private static int _cachedLogCount;
        private static bool _isCacheDirty = false;

        // Maximum number of logs to display initially (reduced again for better performance)
        private const int MaxInitialLogs = 250; // Reduced from 500 to 250

        // Maximum number of blocks for different operational modes
        private const int MaxBlocksForInitialLoad = 5000; // Reduced from 10000 to 5000
        private const int MaxBlocksForHighVolume = 300;  // Much more aggressive for active view
        private const int MaxBlocksForNormalOperation = 500;

        public RichTextBindingBehavior()
        {
            _updateTimer = new DispatcherTimer
            {
                Interval = _updateInterval
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            
            // Add a separate timer for trimming during continuous operation
            _trimTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2) // Check every 2 seconds during active operation
            };
            _trimTimer.Tick += TrimTimer_Tick;
        }
        
        private void TrimTimer_Tick(object? sender, EventArgs e)
        {
            if (AssociatedObject?.Document == null) return;
            
            // Check if we're in high volume logging
            var currentBlockCount = AssociatedObject.Document.Blocks.Count;
            
            // Check growth rate to detect high volume operations
            if (currentBlockCount - _blockCountLastCheck > 50) // If more than 50 blocks added in 2 seconds
            {
                _inHighVolumeMode = true;
            }
            else if (_inHighVolumeMode && currentBlockCount - _blockCountLastCheck < 10)
            {
                // Exit high volume mode if things have slowed down
                _inHighVolumeMode = false;
            }
            
            // Store current count for next check
            _blockCountLastCheck = currentBlockCount;
            
            // Perform trimming if needed
            CheckAndTrimExcessiveBlocks();
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (_needsUpdate && (DateTime.Now - _lastUpdateTime) > _updateInterval)
            {
                ProcessPendingUpdates();
                _lastUpdateTime = DateTime.Now;
                _needsUpdate = _pendingTexts.Count > 0;

                // Mark cache as dirty when updates happen after initial load
                if (_initialLoadCompleted && _needsUpdate)
                {
                    _isCacheDirty = true;
                }
            }
        }

        public ObservableCollection<string> TextCollection
        {
            get { return (ObservableCollection<string>)GetValue(TextCollectionProperty); }
            set { SetValue(TextCollectionProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            _updateTimer.Start();
            _trimTimer.Start();

            // Set maximum document size to improve performance
            if (AssociatedObject != null)
            {
                // Disable TextFormatter for performance
                TextOptions.SetTextFormattingMode(AssociatedObject, TextFormattingMode.Display);

                // Enable UI virtualization
                AssociatedObject.UseLayoutRounding = true;
                AssociatedObject.SnapsToDevicePixels = true;

                // Cache the rendering where possible
                RenderOptions.SetCachingHint(AssociatedObject, CachingHint.Cache);
                RenderOptions.SetBitmapScalingMode(AssociatedObject, BitmapScalingMode.NearestNeighbor);

                // Reduce memory pressure
                AssociatedObject.IsDocumentEnabled = true;
                AssociatedObject.IsUndoEnabled = false;
                AssociatedObject.Background = Brushes.Transparent;
                
                // Disable other features for better performance
                AssociatedObject.SpellCheck.IsEnabled = false;
                AssociatedObject.AcceptsReturn = false;
                AssociatedObject.AcceptsTab = false;
            }

            InitializeRichTextBox();
        }

        protected override void OnDetaching()
        {
            if (TextCollection != null)
            {
                TextCollection.CollectionChanged -= OnCollectionChanged;
            }

            _updateTimer.Stop();
            _trimTimer.Stop();

            // Cache the document before detaching if it's worth caching
            // but only if not in high volume mode
            if (!_inHighVolumeMode && AssociatedObject?.Document != null && AssociatedObject.Document.Blocks.Count > 100)
            {
                if (!_isCacheDirty)
                {
                    // Only cache if we haven't marked it as dirty
                    _cachedDocumentRef = new WeakReference<FlowDocument>(AssociatedObject.Document);
                    _cachedLogCount = _loadedLogCount;
                }
                else
                {
                    // If cache is dirty, don't keep the reference
                    _cachedDocumentRef = null;
                    _cachedLogCount = 0;
                }
            }
            else
            {
                // Don't cache during high volume operations
                _cachedDocumentRef = null;
                _cachedLogCount = 0;
            }

            // Help the GC by explicitly removing large objects
            if (AssociatedObject != null)
            {
                AssociatedObject.Document = new FlowDocument();
                GC.Collect(2, GCCollectionMode.Optimized, false); // Help with large memory release
            }

            base.OnDetaching();
        }

        private static void OnTextCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not RichTextBindingBehavior behavior)
            {
                throw new Exception("Was not able to cast the dependency object to a RichTextBindingBehavior");
            }

            if (e.OldValue is ObservableCollection<string> oldCollection)
            {
                oldCollection.CollectionChanged -= behavior.OnCollectionChanged;
            }

            if (e.NewValue is ObservableCollection<string> newCollection)
            {
                newCollection.CollectionChanged += behavior.OnCollectionChanged;
                behavior.InitializeRichTextBox();
            }
        }

        private void InitializeRichTextBox()
        {
            if (AssociatedObject == null || TextCollection == null) return;

            _initialLoadCompleted = false;
            _pendingTexts.Clear();
            _inHighVolumeMode = false;
            _blockCountLastCheck = 0;

            // Skip restore from cache if in high volume mode - always fresh start for better performance
            bool restoredFromCache = false;

            if (!_inHighVolumeMode && _cachedDocumentRef != null &&
                _cachedDocumentRef.TryGetTarget(out var cachedDocument) &&
                TextCollection.Count == _cachedLogCount &&
                !_isCacheDirty)
            {
                try
                {
                    // Clone the document to avoid sharing
                    var range = new TextRange(cachedDocument.ContentStart, cachedDocument.ContentEnd);
                    var ms = new System.IO.MemoryStream();
                    range.Save(ms, DataFormats.Xaml);
                    ms.Seek(0, System.IO.SeekOrigin.Begin);

                    // Create a new document and load the content
                    var newDocument = new FlowDocument()
                    {
                        PageWidth = 1000,
                        Background = Brushes.Transparent,
                        LineHeight = 1 // Tighter line spacing for performance
                    };
                    var newRange = new TextRange(newDocument.ContentStart, newDocument.ContentEnd);
                    newRange.Load(ms, DataFormats.Xaml);

                    // Set the document
                    AssociatedObject.Document = newDocument;
                    _loadedLogCount = _cachedLogCount;
                    restoredFromCache = true;

                    // Force layout and scroll to end
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                    {
                        ScrollToBottom(AssociatedObject);
                    }));
                }
                catch
                {
                    // If restoring fails, continue with normal initialization
                    restoredFromCache = false;
                }
            }

            if (!restoredFromCache)
            {
                // Create new document with optimized settings
                AssociatedObject.Document = new FlowDocument()
                {
                    PageWidth = 1000, // Prevents text wrapping which improves performance
                    Background = Brushes.Transparent,
                    LineHeight = 1 // Tighter line spacing for performance
                };

                _loadedLogCount = 0;

                // If there are existing logs in the collection, load them
                if (TextCollection.Count > 0)
                {
                    int logsCount = TextCollection.Count;

                    // For large collections, only load the most recent logs initially
                    var logsList = TextCollection.ToList();

                    if (logsList.Count > MaxInitialLogs)
                    {
                        // Only load the most recent logs (much fewer than before)
                        foreach (var text in logsList.Skip(logsList.Count - MaxInitialLogs))
                        {
                            _pendingTexts.Enqueue(text);
                        }

                        _loadedLogCount = MaxInitialLogs;
                    }
                    else
                    {
                        // Enqueue all logs for processing
                        foreach (var text in logsList)
                        {
                            _pendingTexts.Enqueue(text);
                        }

                        _loadedLogCount = logsCount;
                    }

                    _needsUpdate = true;

                    // Process everything at once for the initial load
                    ProcessInitialLoad();
                }
            }

            _initialLoadCompleted = true;
            _isCacheDirty = false;
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (AssociatedObject != null)
                {
                    AssociatedObject.Document.Blocks.Clear();
                    _pendingTexts.Clear();
                    _loadedLogCount = 0;
                    _isCacheDirty = true;
                    _inHighVolumeMode = false;
                }
                return;
            }

            // Only handle new items once initialization is complete
            if (_initialLoadCompleted && (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace))
            {
                // Check for high volume operations
                if (e.NewItems?.Count > 20)
                {
                    _inHighVolumeMode = true;
                }
                
                // Add new logs to the pending queue
                foreach (string item in e.NewItems ?? Array.Empty<object>())
                {
                    _pendingTexts.Enqueue(item);
                    _loadedLogCount++;
                }
                _needsUpdate = true;
                _isCacheDirty = true;
            }
        }
        
        private void CheckAndTrimExcessiveBlocks()
        {
            if (AssociatedObject?.Document == null) return;
            
            var document = AssociatedObject.Document;
            
            // Set the maximum block count based on the operation mode
            int maxBlocks = _inHighVolumeMode ? 
                MaxBlocksForHighVolume : 
                (_initialLoadCompleted ? MaxBlocksForNormalOperation : MaxBlocksForInitialLoad);
            
            // Aggressively trim if we have too many blocks
            if (document.Blocks.Count > maxBlocks)
            {
                int blocksToRemove = document.Blocks.Count - maxBlocks;
                int removed = 0;
                
                // Remove blocks in larger batches for performance
                while (removed < blocksToRemove && document.Blocks.Count > maxBlocks)
                {
                    // Remove the first block (oldest log)
                    document.Blocks.Remove(document.Blocks.FirstBlock);
                    _loadedLogCount--;
                    removed++;
                    
                    // Every 25 blocks, give the UI a chance to update
                    if (removed % 25 == 0 && _inHighVolumeMode)
                    {
                        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);
                    }
                }
                
                // Add a message at the top if we removed logs
                if (removed > 0 && _inHighVolumeMode)
                {
                    var infoMessage = new Paragraph();
                    var infoRun = new Run($"[Trimmed {removed} old log entries for performance]")
                    {
                        Foreground = new SolidColorBrush(Colors.Orange),
                        FontWeight = FontWeights.Bold
                    };
                    infoMessage.Inlines.Add(infoRun);
                    document.Blocks.InsertBefore(document.Blocks.FirstBlock, infoMessage);
                }
            }
        }

        private void ProcessInitialLoad()
        {
            if (AssociatedObject == null || _pendingTexts.Count == 0) return;

            var document = AssociatedObject.Document;

            // Process all logs in smaller batches for faster processing
            const int initialLoadBatchSize = 25; // Reduced from 50 to 25
            int processedCount = 0;

            while (_pendingTexts.Count > 0)
            {
                // Process in batches
                for (int i = 0; i < initialLoadBatchSize && _pendingTexts.Count > 0; i++)
                {
                    string text = _pendingTexts.Dequeue();
                    processedCount++;

                    try
                    {
                        var paragraph = text.ToRichText();
                        document.Blocks.Add(paragraph);
                    }
                    catch (Exception)
                    {
                        // If formatting fails, add as plain text (simpler approach)
                        var paragraph = new Paragraph();
                        paragraph.Inlines.Add(new Run(text));
                        document.Blocks.Add(paragraph);
                    }
                }

                // Give the UI a chance to update for responsiveness
                if (_pendingTexts.Count > 0)
                {
                    // Allow UI to process events between batches for responsiveness
                    var frame = new DispatcherFrame();
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                        new DispatcherOperationCallback(obj =>
                        {
                            ((DispatcherFrame)obj).Continue = false;
                            return null;
                        }), frame);
                    Dispatcher.PushFrame(frame);
                }
                
                // Check if we need to trim during a long initial load
                if (document.Blocks.Count > MaxBlocksForInitialLoad)
                {
                    CheckAndTrimExcessiveBlocks();
                }
            }

            // Always scroll to the bottom after initial load
            ScrollToBottom(AssociatedObject);
        }

        private void ProcessPendingUpdates()
        {
            if (AssociatedObject == null || _pendingTexts.Count == 0) return;

            // Get the document
            var document = AssociatedObject.Document;
            
            // Check for high volume operations
            if (_pendingTexts.Count > MaxItemsPerUpdate * 5)
            {
                _inHighVolumeMode = true;
            }

            // Process only a limited number of items per update to maintain responsiveness
            int itemsToProcess = Math.Min(_pendingTexts.Count, MaxItemsPerUpdate);

            // Process a batch of logs
            for (int i = 0; i < itemsToProcess && _pendingTexts.Count > 0; i++)
            {
                string text = _pendingTexts.Dequeue();

                try
                {
                    // Use the ToRichText extension for proper color formatting
                    var paragraph = text.ToRichText();
                    document.Blocks.Add(paragraph);
                }
                catch (Exception)
                {
                    // If formatting fails, add as plain text
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run(text));
                    document.Blocks.Add(paragraph);
                }
            }

            // Check if we need to trim logs for continuous operation mode
            CheckAndTrimExcessiveBlocks();

            // Always scroll to the bottom
            ScrollToBottom(AssociatedObject);
        }

        private void ScrollToBottom(RichTextBox richTextBox)
        {
            try
            {
                // Force layout update to ensure accurate scrolling
                richTextBox.UpdateLayout();
                richTextBox.ScrollToEnd();
            }
            catch
            {
                // Ignore scrolling errors
            }
        }
    }
}