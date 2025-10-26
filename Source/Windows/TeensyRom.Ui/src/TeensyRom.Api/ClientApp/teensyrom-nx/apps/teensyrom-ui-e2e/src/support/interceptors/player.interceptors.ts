/// <reference types="cypress" />

import type { LaunchFileResponse, LaunchRandomResponse, FileItemDto } from '@teensyrom-nx/data-access/api-client';
import type { CyHttpMessages } from 'cypress/types/net-stubbing';
import { PLAYER_ENDPOINTS, INTERCEPT_ALIASES, createProblemDetailsResponse } from '../constants/api.constants';
import { TEST_FILES, TEST_PATHS } from '../constants/storage.constants';
import type MockFilesystem from '../test-data/mock-filesystem/mock-filesystem';
import { generateFileItem } from '../test-data/generators/storage.generators';

export interface InterceptLaunchFileOptions {
  /** Mock filesystem to look up file item. If not provided, generates a default file item */
  filesystem?: MockFilesystem;
  /** When true, return HTTP 500 error to simulate launch failure */
  errorMode?: boolean;
  /** Response delay in milliseconds (default: 0) */
  responseDelayMs?: number;
}

export interface InterceptLaunchRandomOptions {
  /** Mock filesystem to select a random file from. If not provided, generates a default file item */
  filesystem?: MockFilesystem;
  /** Specific file to return instead of random selection. When provided, overrides random behavior for deterministic testing */
  selectedFile?: FileItemDto;
  /** When true, return HTTP 500 error to simulate launch failure */
  errorMode?: boolean;
  /** Response delay in milliseconds (default: 0) */
  responseDelayMs?: number;
}

function getQueryParam(request: CyHttpMessages.IncomingHttpRequest, key: string): unknown {
  const query = (request.query ?? {}) as Record<string, unknown>;
  return query[key];
}

function resolvePath(queryParam: unknown, fallback = TEST_PATHS.ROOT): string {
  if (typeof queryParam === 'string' && queryParam.length > 0) {
    return queryParam;
  }

  if (Array.isArray(queryParam) && queryParam[0]) {
    return queryParam[0];
  }

  return fallback;
}

/**
 * Intercepts POST /devices/{deviceId}/storage/{storageType}/launch - File launch endpoint
 * Route matches any deviceId and storageType via wildcard pattern from PLAYER_ENDPOINTS.
 *
 * The FilePath is passed as a query parameter: ?FilePath=/games/Pac-Man%20(J1).crt
 */
export function interceptLaunchFile(options: InterceptLaunchFileOptions = {}): void {
  const { filesystem, errorMode = false, responseDelayMs = 0 } = options;

  cy.intercept(
    PLAYER_ENDPOINTS.LAUNCH_FILE.method,
    PLAYER_ENDPOINTS.LAUNCH_FILE.pattern,
    (req) => {
      if (errorMode) {
        req.reply(createProblemDetailsResponse(500, 'Failed to launch file'));
        return;
      }

      const filePath = resolvePath(getQueryParam(req, 'FilePath'), TEST_PATHS.ROOT);
      let launchedFile;

      if (filesystem) {
        // Look up file from the filesystem by getting the directory and searching for the file
        const dirPath = filePath.substring(0, filePath.lastIndexOf(TEST_PATHS.SEPARATOR)) || TEST_PATHS.ROOT;
        const fileName = filePath.substring(filePath.lastIndexOf(TEST_PATHS.SEPARATOR) + 1);
        const directoryResponse = filesystem.getDirectory(dirPath);

        const file = directoryResponse.storageItem.files?.find((f) => f.name === fileName);
        if (file) {
          launchedFile = file;
        } else {
          // Fallback if file not found in filesystem
          launchedFile = generateFileItem({
            path: filePath,
            name: fileName || TEST_FILES.DEFAULT_UNKNOWN_FILE,
          });
        }
      } else {
        // Generate a default file item if no filesystem provided
        launchedFile = generateFileItem({
          path: filePath,
          name: filePath.split(TEST_PATHS.SEPARATOR).pop() || TEST_FILES.DEFAULT_UNKNOWN_FILE,
        });
      }

      const response: LaunchFileResponse = {
        message: `Successfully launched ${launchedFile.name}`,
        launchedFile,
        isCompatible: launchedFile.isCompatible ?? true,
      };

      req.reply({
        delay: responseDelayMs,
        statusCode: 200,
        body: response,
      });
    }
  ).as(INTERCEPT_ALIASES.LAUNCH_FILE);
}

/**
 * Intercepts POST /devices/{deviceId}/storage/{storageType}/launch/random - Random file launch endpoint
 * Route matches any deviceId and storageType via wildcard pattern from PLAYER_ENDPOINTS.
 *
 * Query parameters: Scope, FilterType, StartingDirectory
 */
export function interceptLaunchRandom(options: InterceptLaunchRandomOptions = {}): void {
  const { filesystem, selectedFile, errorMode = false, responseDelayMs = 0 } = options;

  cy.intercept(
    PLAYER_ENDPOINTS.LAUNCH_RANDOM.method,
    PLAYER_ENDPOINTS.LAUNCH_RANDOM.pattern,
    (req) => {
      if (errorMode) {
        req.reply(createProblemDetailsResponse(500, 'Failed to launch random file'));
        return;
      }

      let launchedFile;

      // Prioritize selectedFile for deterministic testing
      if (selectedFile) {
        launchedFile = selectedFile;
      } else if (filesystem) {
        // Get all files from the filesystem and pick a random one
        const startingDir = resolvePath(getQueryParam(req, 'StartingDirectory'), TEST_PATHS.ROOT);
        const directoryResponse = filesystem.getDirectory(startingDir);
        const availableFiles = directoryResponse.storageItem.files || [];

        if (availableFiles.length > 0) {
          launchedFile = availableFiles[Math.floor(Math.random() * availableFiles.length)];
        } else {
          // Fallback if no files available
          launchedFile = generateFileItem();
        }
      } else {
        // Generate a default random file item if no filesystem provided
        launchedFile = generateFileItem();
      }

      const response: LaunchRandomResponse = {
        message: `Successfully launched random file ${launchedFile.name}`,
        launchedFile,
        isCompatible: launchedFile.isCompatible ?? true,
      };

      req.reply({
        delay: responseDelayMs,
        statusCode: 200,
        body: response,
      });
    }
  ).as(INTERCEPT_ALIASES.LAUNCH_RANDOM);
}
