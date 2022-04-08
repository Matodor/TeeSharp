using System.Threading;
using System.Threading.Tasks;

namespace TeeSharp.Commands;

public delegate Task CommandHandler(
    CommandArgs args,
    CommandContext context,
    CancellationToken cancellationToken);
