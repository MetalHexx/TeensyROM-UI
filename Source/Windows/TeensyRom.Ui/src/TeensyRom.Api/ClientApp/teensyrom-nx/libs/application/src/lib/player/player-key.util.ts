// Player key utility to allow future expansion beyond simple deviceId keys.
export type PlayerKey = string;

export const PlayerKeyUtil = {
  create(deviceId: string): PlayerKey {
    return deviceId as PlayerKey;
  },

  parse(key: PlayerKey): { deviceId: string } {
    return { deviceId: key };
  },
} as const;
