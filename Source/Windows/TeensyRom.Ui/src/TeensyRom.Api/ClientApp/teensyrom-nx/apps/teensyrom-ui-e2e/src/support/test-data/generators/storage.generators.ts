import { faker } from '../faker-config';
import {
  FileItemType,
  type FileItemDto,
  type FileItemType as FileItemTypeUnion,
} from '@teensyrom-nx/data-access/api-client';
import MockFilesystem from '../mock-filesystem/mock-filesystem';
import type { MockFile } from '../mock-filesystem/filesystem.types';

const FAVORITE_DIRECTORIES: Array<{ path: string }> = [
  { path: '/favorites/games' },
  { path: '/favorites/music' },
  { path: '/favorites/images' },
];

const DEFAULT_FILE_SIZE_RANGE = {
  min: 1024,
  max: 102400,
};

const DEFAULT_PLAY_LENGTHS = ['02:30', '03:15', '04:05'];

/**
 * Generates a deterministic FileItemDto with sensible defaults.
 * Callers can override any property to tailor the file for a directory.
 */
export function generateFileItem(overrides: Partial<FileItemDto> = {}): FileItemDto {
  const name = overrides.name ?? faker.system.fileName();
  const parentPath = overrides.parentPath ?? '/';
  const inferredPath = `${parentPath}/${name}`.replace(/\/\/+/g, '/');

  return {
    name,
    path: overrides.path ?? inferredPath,
    size: overrides.size ?? faker.number.int(DEFAULT_FILE_SIZE_RANGE),
    type: overrides.type ?? FileItemType.Game,
    isFavorite: overrides.isFavorite ?? false,
    isCompatible: overrides.isCompatible ?? true,
    title: overrides.title ?? faker.lorem.words(3),
    creator: overrides.creator ?? faker.person.fullName(),
    releaseInfo:
      overrides.releaseInfo ??
      faker.date
        .past({ years: 30 })
        .getFullYear()
        .toString(),
    description: overrides.description ?? faker.lorem.sentence(),
    shareUrl: overrides.shareUrl ?? faker.internet.url(),
    metadataSource: overrides.metadataSource ?? 'HVSC',
    meta1: overrides.meta1 ?? faker.music.genre(),
    meta2: overrides.meta2 ?? faker.word.sample(),
    links: overrides.links ?? [],
    tags: overrides.tags ?? [],
    youTubeVideos: overrides.youTubeVideos ?? [],
    competitions: overrides.competitions ?? [],
    avgRating: overrides.avgRating ?? null,
    ratingCount: overrides.ratingCount ?? 0,
    metadataSourcePath:
      overrides.metadataSourcePath ?? `${parentPath}/${name}`.replace(/\/\/+/g, '/'),
    parentPath,
    playLength: overrides.playLength ?? faker.helpers.arrayElement(DEFAULT_PLAY_LENGTHS),
    subtuneLengths: overrides.subtuneLengths ?? [],
    startSubtuneNum: overrides.startSubtuneNum ?? 1,
    images: overrides.images ?? [],
  };
}

/**
 * Generates a flat games directory using realistic CRT file names and sizes.
 */
export function generateGamesDirectory(seed: number): MockFile[] {
  faker.seed(seed);

  const games = [
    { name: '10th Frame.crt', title: '10th Frame' },
    { name: '1942 (Music v1).crt', title: '1942' },
    { name: 'Donkey Kong (Ocean).crt', title: 'Donkey Kong' },
    { name: 'Pac-Man (J1).crt', title: 'Pac-Man' },
    { name: 'Mario Bros (Ocean) (J1).crt', title: 'Mario Bros' },
  ];

  const sizePool = [64200, 80200, 88200];

  return games.map(({ name, title }) =>
    generateFileItem({
      name,
      title,
      path: `/games/${name}`,
      parentPath: '/games',
      type: FileItemType.Game,
      size: faker.helpers.arrayElement(sizePool),
    }),
  );
}

/**
 * Generates a musician directory with deterministic SID files.
 */
export function generateMusicianDirectory(
  letter: string,
  artistName: string,
  seed: number,
): MockFile[] {
  faker.seed(seed + letter.charCodeAt(0));

  const trackNames = ['Alpha.sid', 'Dreams.sid', 'Neon_Thrills.sid', 'Proxima.sid'];
  const basePath = `/music/MUSICIANS/${letter}/${artistName}`;

  return trackNames.map((name) =>
    generateFileItem({
      name,
      path: `${basePath}/${name}`,
      parentPath: basePath,
      type: FileItemType.Song,
      creator: artistName,
      size: faker.number.int({ min: 6700, max: 22900 }),
    }),
  );
}

/**
 * Generates an images directory with representative KLA artwork.
 */
export function generateImagesDirectory(seed: number): MockFile[] {
  faker.seed(seed);

  const imageNames = ['ChrisCornell1.kla', 'Dio2.kla', 'SonicTheHedgehog.kla'];

  return imageNames.map((name) =>
    generateFileItem({
      name,
      path: `/images/${name}`,
      parentPath: '/images',
      type: FileItemType.Image,
      size: 9800,
    }),
  );
}

/**
 * Creates a seeded MockFilesystem populated with the core storage structure.
 */
export function createMockFilesystem(seed = 12345): MockFilesystem {
  const filesystem = new MockFilesystem(seed);

  // Add root directory with top-level subdirectories
  // This is needed for bootstrap calls that fetch the root directory
  filesystem.addDirectory('/', {
    path: '/',
    files: [],
    subdirectories: ['games', 'music', 'images', 'favorites'],
  });

  const games = generateGamesDirectory(seed);
  filesystem.addDirectory('/games', {
    path: '/games',
    files: games,
    subdirectories: ['Extras', 'Large', 'MultiLoad64'],
  });

  // Add intermediate music directories for proper navigation
  filesystem.addDirectory('/music', {
    path: '/music',
    files: [],
    subdirectories: ['MUSICIANS'],
  });

  filesystem.addDirectory('/music/MUSICIANS', {
    path: '/music/MUSICIANS',
    files: [],
    subdirectories: ['L'],
  });

  filesystem.addDirectory('/music/MUSICIANS/L', {
    path: '/music/MUSICIANS/L',
    files: [],
    subdirectories: ['LukHash'],
  });

  const lukHashTracks = generateMusicianDirectory('L', 'LukHash', seed);
  filesystem.addDirectory('/music/MUSICIANS/L/LukHash', {
    path: '/music/MUSICIANS/L/LukHash',
    files: lukHashTracks,
    subdirectories: [],
  });

  const images = generateImagesDirectory(seed);
  filesystem.addDirectory('/images', {
    path: '/images',
    files: images,
    subdirectories: [],
  });

  // Add favorites parent directory
  filesystem.addDirectory('/favorites', {
    path: '/favorites',
    files: [],
    subdirectories: ['games', 'music', 'images'],
  });

  FAVORITE_DIRECTORIES.forEach(({ path }) => {
    filesystem.addDirectory(path, {
      path,
      files: [],
      subdirectories: [],
    });
  });

  return filesystem;
}

/**
 * Helper for determining favorites path outside of filesystem context.
 */
export function getFavoritesPathByType(type: FileItemTypeUnion): string {
  switch (type) {
    case FileItemType.Song:
      return '/favorites/music';
    case FileItemType.Image:
      return '/favorites/images';
    case FileItemType.Game:
    case FileItemType.Hex:
    case FileItemType.Unknown:
    default:
      return '/favorites/games';
  }
}
