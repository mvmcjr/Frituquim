# 🍟 Frituquim

**Frituquim** é uma aplicação Windows moderna e eficiente para conversão de vídeos, download de mídia e extração de frames, construída com WPF e .NET 8.

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)
![WPF](https://img.shields.io/badge/WPF-UI-purple)
![Windows](https://img.shields.io/badge/Windows-Only-lightblue)
![License](https://img.shields.io/badge/License-MIT-green)

## ✨ Funcionalidades

### 🎥 Conversão de Vídeos
- **Conversão para MP4** com alta qualidade
- **Aceleração por hardware** suportada:
  - NVIDIA GPU (NVENC)
  - Intel Quick Sync
  - CPU (software)
- **Processamento em lote** com até 3 arquivos simultâneos
- **Progresso em tempo real** com ETA preciso por arquivo
- **Interface moderna** com WPF-UI

### 📥 Download de Mídia
- **Download de vídeos** de plataformas populares
- **Download apenas de áudio** em alta qualidade
- **URLs múltiplas** suportadas
- **Integração com yt-dlp** para máxima compatibilidade

### 🖼️ Extração de Frames
- **Extração de quadros** de vídeos em alta resolução
- **Controle de intervalo** personalizável
- **Formatos de saída** flexíveis
- **Processamento eficiente** com FFmpeg

## 🚀 Instalação

### Pré-requisitos
- **Windows 10/11** (64-bit)
- **.NET 8 Runtime** ou superior
- **FFmpeg** instalado no sistema

### Download
1. Acesse a página de [Releases](../../releases)
2. Baixe a versão mais recente
3. Execute o instalador ou extraia o arquivo ZIP
4. Execute `Frituquim.exe`

### Instalação do FFmpeg
O Frituquim requer o FFmpeg para funcionar. Você pode:

1. **Instalação automática**: O aplicativo tentará localizar o FFmpeg automaticamente
2. **Instalação manual**: 
   - Baixe o FFmpeg de [ffmpeg.org](https://ffmpeg.org/download.html)
   - Adicione o executável ao PATH do sistema
   - Ou coloque `ffmpeg.exe` na pasta do Frituquim

## ⚙️ Tecnologias

### Core
- **.NET 8** - Framework principal
- **WPF** - Interface de usuário
- **WPF-UI** - Componentes modernos de UI
- **CommunityToolkit.MVVM** - Padrão MVVM

### Processamento
- **FFmpeg** - Conversão e processamento de vídeo
- **yt-dlp** - Download de mídia
- **System.Reactive** - Programação reativa para performance

### Outras Bibliotecas
- **EnumerableAsyncProcessor** - Processamento paralelo eficiente
- **Microsoft.Extensions.Hosting** - Injeção de dependência
- **CalcBinding** - Binding avançado

## 🏗️ Arquitetura

### Padrões Utilizados
- **MVVM (Model-View-ViewModel)** para separação de responsabilidades
- **Dependency Injection** para acoplamento baixo
- **Reactive Programming** para updates de UI eficientes
- **Observer Pattern** para progresso em tempo real

### Estrutura do Projeto
```
Frituquim/
├── Models/           # Modelos de dados e lógica de negócio
├── ViewModels/       # ViewModels para binding
├── Views/            # Páginas e controles da UI
├── Helpers/          # Utilitários (FFmpeg, yt-dlp)
├── Services/         # Serviços da aplicação
├── Converters/       # Conversores de dados para UI
└── Assets/           # Recursos (ícones, imagens)
```

## 🔧 Desenvolvimento

### Pré-requisitos de Desenvolvimento
- **Visual Studio 2022** ou **JetBrains Rider**
- **.NET 8 SDK**
- **Git**

### Clonando o Repositório
```bash
git clone https://github.com/seu-usuario/frituquim.git
cd frituquim
```

### Restaurando Dependências
```bash
dotnet restore
```

### Executando
```bash
dotnet run --project Frituquim/Frituquim.csproj
```

### Build de Release
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## 🤝 Contribuindo

Contribuições são bem-vindas! Por favor:

1. **Fork** o projeto
2. Crie uma **branch** para sua feature (`git checkout -b feature/nova-funcionalidade`)
3. **Commit** suas mudanças (`git commit -am 'Adiciona nova funcionalidade'`)
4. **Push** para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um **Pull Request**

### Convenções de Código
- Use **C# 12** e recursos modernos
- Siga os padrões do **.NET**

## 📝 Licença

Este projeto está licenciado sob a **Licença MIT** - veja o arquivo [LICENSE](LICENSE) para detalhes.

## 🙏 Agradecimentos

- **FFmpeg** - Pelo excelente framework de processamento multimídia
- **yt-dlp** - Pela ferramenta robusta de download
- **WPF-UI** - Pelos componentes de interface modernos
- **Microsoft** - Pelo .NET e WPF

## 📞 Suporte

- 🐛 **Bugs**: Abra uma [issue](../../issues)
- 💡 **Sugestões**: Use as [discussions](../../discussions)

---

<div align="center">

**Feito com ❤️ no Brasil**

[⭐ Deixe uma estrela se gostou do projeto!](../../stargazers)

</div>
