import { Provider } from '@angular/core';
import { PLAYER_CONTEXT } from './player-context.interface';
import { PlayerContextService } from './player-context.service';

export const PLAYER_CONTEXT_PROVIDER: Provider = {
  provide: PLAYER_CONTEXT,
  useExisting: PlayerContextService,
};

export const PLAYER_CONTEXT_PROVIDERS: Provider[] = [PlayerContextService, PLAYER_CONTEXT_PROVIDER];
