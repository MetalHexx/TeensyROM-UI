import 'zone.js';
import { getTestBed } from '@angular/core/testing';
import {
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting,
} from '@angular/platform-browser-dynamic/testing';

// Polyfill for MSW v2 - TransformStream is not available in jsdom
if (!globalThis.TransformStream) {
  const { TransformStream } = await import('node:stream/web');
  globalThis.TransformStream = TransformStream;
}

// Initialize Angular testing environment
getTestBed().initTestEnvironment(BrowserDynamicTestingModule, platformBrowserDynamicTesting());
