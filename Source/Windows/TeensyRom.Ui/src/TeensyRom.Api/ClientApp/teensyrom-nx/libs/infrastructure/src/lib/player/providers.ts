import { PLAYER_SERVICE } from '@teensyrom-nx/domain';
import { PlayerService } from './player.service';
import { PlayerApiService, Configuration } from '@teensyrom-nx/data-access/api-client';

export const PLAYER_API_CLIENT_PROVIDER = {
  provide: PlayerApiService,
  useFactory: () => {
    const config = new Configuration({ basePath: 'http://localhost:5168' });
    return new PlayerApiService(config);
  },
};

export const PLAYER_SERVICE_PROVIDER = {
  provide: PLAYER_SERVICE,
  useClass: PlayerService,
  deps: [PlayerApiService],
};
