namespace PdfUnificador;

/// <summary>
/// Paleta de cores e constantes visuais do app — dark theme industrial/refinado.
/// </summary>
internal static class AppTheme
{
    // ── Backgrounds ──────────────────────────────────────────────
    public static readonly Color BgDeep    = Color.FromArgb(13,  13,  18);   // #0D0D12
    public static readonly Color BgPanel   = Color.FromArgb(20,  20,  28);   // #14141C
    public static readonly Color BgCard    = Color.FromArgb(28,  28,  40);   // #1C1C28
    public static readonly Color BgHover   = Color.FromArgb(38,  38,  55);   // #262637
    public static readonly Color BgInput   = Color.FromArgb(24,  24,  34);   // #181822

    // ── Accent ───────────────────────────────────────────────────
    public static readonly Color Accent    = Color.FromArgb(99,  179, 237);  // #63B3ED — azul-gelo
    public static readonly Color AccentDim = Color.FromArgb(49,  109, 167);  // mais escuro
    public static readonly Color Danger    = Color.FromArgb(252, 129, 129);  // #FC8181

    // ── Text ──────────────────────────────────────────────────────
    public static readonly Color TextPrimary   = Color.FromArgb(237, 237, 245); // quase-branco
    public static readonly Color TextSecondary = Color.FromArgb(140, 140, 165);
    public static readonly Color TextMuted     = Color.FromArgb(75,  75,  100);

    // ── Border ───────────────────────────────────────────────────
    public static readonly Color Border    = Color.FromArgb(45,  45,  65);
    public static readonly Color BorderFocus = Color.FromArgb(99, 179, 237);

    // ── Fonts ─────────────────────────────────────────────────────
    public static readonly Font FontTitle   = new("Segoe UI", 18f, FontStyle.Bold);
    public static readonly Font FontSubtitle= new("Segoe UI", 9f,  FontStyle.Regular);
    public static readonly Font FontBody    = new("Segoe UI", 9f,  FontStyle.Regular);
    public static readonly Font FontBodyBold= new("Segoe UI", 9f,  FontStyle.Bold);
    public static readonly Font FontMono    = new("Consolas",  8.5f, FontStyle.Regular);
    public static readonly Font FontSmall   = new("Segoe UI", 7.5f, FontStyle.Regular);
}
