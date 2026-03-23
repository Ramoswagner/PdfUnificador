# 📄 PDF Unificador

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D6?style=for-the-badge&logo=windows)
![Build](https://img.shields.io/github/actions/workflow/status/SEU_USUARIO/PdfUnificador/build.yml?style=for-the-badge&label=CI%2FCD)
![License](https://img.shields.io/badge/license-MIT-green?style=for-the-badge)

**Ferramenta Windows para combinar múltiplos PDFs em um único arquivo.**  
Interface dark, drag-and-drop, self-contained — sem instalação de .NET.

</div>

---

## ✨ Funcionalidades

| Recurso | Descrição |
|---|---|
| **Drag & Drop** | Arraste os PDFs diretamente para a janela |
| **Reordenação** | Mova arquivos para cima/baixo antes de unificar |
| **Metadados** | Exibe número de páginas e tamanho de cada arquivo |
| **Validação** | Detecta PDFs corrompidos ou protegidos antes de processar |
| **Cancelamento** | Cancele a operação a qualquer momento |
| **Self-contained** | Executável único, sem dependências externas |
| **Dark Theme** | Interface escura com suporte a DPI alto |

---

## 🚀 Como usar

### Opção 1 — Download direto (recomendado)

1. Vá em [**Releases**](../../releases) e baixe o arquivo `PdfUnificador-vX.X.X-win-x64.zip`
2. Extraia o ZIP
3. Execute `PdfUnificador.exe`

> ✅ **Sem instalação** — totalmente self-contained para Windows 10/11 x64.

### Opção 2 — Build local

**Pré-requisitos:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

```bash
# Clone o repositório
git clone https://github.com/SEU_USUARIO/PdfUnificador.git
cd PdfUnificador

# Build
dotnet build src/PdfUnificador/PdfUnificador.csproj --configuration Release

# Publicar (executável único)
dotnet publish src/PdfUnificador/PdfUnificador.csproj \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  --output ./dist
```

---

## 🏗️ Estrutura do projeto

```
PdfUnificador/
│
├── src/
│   └── PdfUnificador/
│       ├── PdfUnificador.csproj   # Definição do projeto (.NET 8 WinForms)
│       ├── Program.cs             # Entry point + dark title bar
│       ├── MainForm.cs            # UI principal (drag-drop, lista, progress)
│       ├── PdfService.cs          # Lógica de merge e validação (PdfSharp)
│       ├── AppTheme.cs            # Paleta de cores e fontes
│       └── app.manifest           # DPI awareness (PerMonitorV2)
│
├── .github/
│   └── workflows/
│       └── build.yml              # CI/CD: build → publish → release
│
└── README.md
```

---

## ⚙️ CI/CD — GitHub Actions

O workflow `build.yml` executa três jobs automaticamente:

```
Push para main  ──►  Build  ──►  Publish (self-contained)
                                        │
Tag v*.*.*  ──────────────────────────►  Release (GitHub Release + ZIP)
```

### Criar uma nova release

```bash
git tag v1.0.0
git push origin v1.0.0
```

O GitHub Actions irá:
1. Compilar o projeto
2. Publicar como `.exe` self-contained
3. Criar um ZIP
4. Publicar automaticamente em **GitHub Releases**

---

## 🛠️ Tecnologias

- **[.NET 8](https://dotnet.microsoft.com/)** + **Windows Forms**
- **[PdfSharp 6.x](https://github.com/empira/PDFsharp)** — manipulação de PDFs (MIT)
- **GitHub Actions** — CI/CD automatizado

---

## 📝 Licença

MIT © 2026 Wagner Ramos — veja [LICENSE](LICENSE) para detalhes.
