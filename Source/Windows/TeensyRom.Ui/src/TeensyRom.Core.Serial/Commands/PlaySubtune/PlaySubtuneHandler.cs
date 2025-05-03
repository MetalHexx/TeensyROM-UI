using MediatR;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Commands.PlaySubtune
{

    public class PlaySubtuneHandler(IPlaySubtuneSerialRoutine playSubtune) : IRequestHandler<PlaySubtuneCommand, PlaySubtuneResult>
    {
        public async Task<PlaySubtuneResult> Handle(PlaySubtuneCommand request, CancellationToken cancellationToken)
        {
            try
            {
                playSubtune.Execute((uint)request.SubtuneIndex);
            }
            catch (TeensyException ex)
            {
                return new PlaySubtuneResult
                {
                    IsSuccess = false,
                    Error = ex.Message
                };
            }
            return new PlaySubtuneResult();
        }
    }
}
