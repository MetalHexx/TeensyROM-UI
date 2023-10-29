using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TeensyRom.Ui.Features.FileTransfer
{
    /// <summary>
    /// Some test data for the view
    /// </summary>
    internal static class FileTreeTestData
    {
        public static List<StorageItemVm> InitializeTestStorageItems()
        {
            return new()
            {
                new DirectoryItemVm
                {
                    Name = "Test DirectoryTest DirectoryTest DirectoryTest DirectoryTest DirectoryTest Directory",
                    Path = "Test Path"
                },
                new DirectoryItemVm
                {
                    Name = "Test Directory 2",
                    Path = "Test Path 2"
                },
                new DirectoryItemVm
                {
                    Name = "Test Directory 3",
                    Path = "Test Path 3"
                },
                new FileItemVm
                {
                    Name = "Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest Test FileTest ",
                    Path = "Test Path"
                },
                new FileItemVm
                {
                    Name = "Test File 2",
                    Path = "Test Path 2"
                },
                new FileItemVm
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
                ,
                new FileItemVm
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
                ,
                new FileItemVm
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
                ,
                new FileItemVm
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
                ,
                new FileItemVm
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
                ,
                new FileItemVm
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
                ,
                new FileItemVm
                {
                    Name = "Test File 3",
                    Path = "Test Path 3"
                }
            };
        }
        public static DirectoryNode InitializeSourceItems()
        {
            return new DirectoryNode()
            {
                Name = @"C:\C64\",
                Children = new ObservableCollection<NodeBase>
                {
                    new DirectoryNode
                    {
                        Name = "Games",
                        Children = new ObservableCollection<NodeBase>
                        {
                            new FileNode { Name = "KingsQuest.prg" },
                            new FileNode { Name = "TestDrive.prg" },
                            new FileNode { Name = "BoulderDash.prg" },
                            new FileNode { Name = "TheSentinel.prg" },
                            new FileNode { Name = "Elite.prg" },
                            new FileNode { Name = "Ghostbusters.prg" },
                            new FileNode { Name = "Wizball.prg" },
                            new FileNode { Name = "ImpossibleMission.prg" },
                            new FileNode { Name = "Barbarian.prg" },
                            new FileNode { Name = "Lemmings.prg" }
                        }
                    },
                    new DirectoryNode
                    {
                        Name = "Sids",
                        Children = new ObservableCollection<NodeBase>
                        {
                            new FileNode { Name = "Paranoid.sid" },

                            new DirectoryNode
                            {
                                Name = "Iron Maiden",
                                Children = new ObservableCollection<NodeBase>
                                {
                                    new FileNode { Name = "AcesHigh.sid" },
                                    new FileNode { Name = "TheTrooper.sid" },
                                    new FileNode { Name = "RunToTheHills.sid" }
                                }
                            },
                            new DirectoryNode
                            {
                                Name = "Pink Floyd",
                                Children = new ObservableCollection<NodeBase>
                                {
                                    new FileNode { Name = "ComfortablyNumb.sid" },
                                    new FileNode { Name = "AnotherBrickInTheWall.sid" },
                                    new FileNode { Name = "WishYouWereHere.sid" },
                                    new FileNode { Name = "Time.sid" }
                                }
                            },
                            new DirectoryNode
                            {
                                Name = "Deep Purple",
                                Children = new ObservableCollection<NodeBase>
                                {
                                    new FileNode { Name = "SmokeOnTheWater.sid" },
                                    new FileNode { Name = "HighwayStar.sid" }
                                }
                            }
                        }
                    }
                }
            };
        }

        public static DirectoryNode InitializeTargetItems()
        {
            return new DirectoryNode()
            {
                Name = @"\sd-card-files\",
                Children = new ObservableCollection<NodeBase>
                {
                    new DirectoryNode
                    {
                        Name = "Games",
                        Children = new ObservableCollection<NodeBase>
                        {
                            new FileNode { Name = "SpaceQuest.prg" },
                            new FileNode { Name = "ManiacMansion.prg" },
                            new FileNode { Name = "ZakMcKracken.prg" },
                            new FileNode { Name = "Paradroid.prg" },
                            new FileNode { Name = "Uridium.prg" },
                            new FileNode { Name = "PrinceOfPersia.prg" },
                            new FileNode { Name = "Turrican.prg" },
                            new FileNode { Name = "WinterGames.prg" },
                            new FileNode { Name = "LastNinja.prg" },
                            new FileNode { Name = "Archon.prg" }
                        }
                    },
                    new DirectoryNode
                    {
                        Name = "Sids",
                        Children = new ObservableCollection<NodeBase>
                        {
                            new FileNode { Name = "HolyDiver.sid" },
                            new FileNode { Name = "Roundabout.sid" },

                            new DirectoryNode
                            {
                                Name = "Rush",
                                Children = new ObservableCollection<NodeBase>
                                {
                                    new FileNode { Name = "TomSawyer.sid" },
                                    new FileNode { Name = "YYZ.sid" },
                                    new FileNode { Name = "Limelight.sid" },
                                    new FileNode { Name = "RedBarchetta.sid" }
                                }
                            },
                            new DirectoryNode
                            {
                                Name = "Led Zeppelin",
                                Children = new ObservableCollection<NodeBase>
                                {
                                    new FileNode { Name = "StairwayToHeaven.sid" },
                                    new FileNode { Name = "BlackDog.sid" },
                                    new FileNode { Name = "WholeLottaLove.sid" },
                                    new FileNode { Name = "RockAndRoll.sid" }
                                }
                            },
                            new DirectoryNode
                            {
                                Name = "Genesis",
                                Children = new ObservableCollection<NodeBase>
                                {
                                    new FileNode { Name = "InvisibleTouch.sid" },
                                    new FileNode { Name = "Mama.sid" },
                                    new FileNode { Name = "FollowYouFollowMe.sid" }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
