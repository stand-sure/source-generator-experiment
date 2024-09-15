namespace SourceGenerator;

internal static class FunctionalHelpers
{
    internal static void Match(this bool match, Action? onTrue = null, Action? onFalse = null)
    {
        onTrue ??= Ignore;
        onFalse ??= Ignore;
        Action act = match ? onTrue : onFalse;
        act();
    }

    private static void Ignore()
    {
    }
}