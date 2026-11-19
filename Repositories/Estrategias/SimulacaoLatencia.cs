namespace Concorrencia.Api.Repositories.Estrategias;

internal static class SimulacaoLatencia
{
    public static Task AguardarAsync(CancellationToken cancellationToken)
    {
        return Task.Delay(Random.Shared.Next(200, 501), cancellationToken);
    }

    public static void Aguardar()
    {
        Thread.Sleep(Random.Shared.Next(200, 501));
    }
}
