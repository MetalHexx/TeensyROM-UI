import '@analogjs/vitest-angular/setup-zone';

import {
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting,
} from '@angular/platform-browser-dynamic/testing';
import { getTestBed } from '@angular/core/testing';

// Polyfills for MSW in Node environment
import { TransformStream } from 'node:stream/web';
import { ReadableStream } from 'node:stream/web';
import { WritableStream } from 'node:stream/web';

// @ts-expect-error - Global assignments for MSW compatibility
globalThis.TransformStream = TransformStream;
// @ts-expect-error - Global assignments for MSW compatibility
globalThis.ReadableStream = ReadableStream;
// @ts-expect-error - Global assignments for MSW compatibility
globalThis.WritableStream = WritableStream;

getTestBed().initTestEnvironment(BrowserDynamicTestingModule, platformBrowserDynamicTesting());
