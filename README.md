# UML-GUI-POO — Trabalho de Programação Orientada a Objetos

Repositório com os jogos desenvolvidos pelo **Grupo 01**, suas documentações e o código-fonte de backup.

## Grupo 01

| Integrantes |
| --- |
| Flávio Storary G. P. de Freitas |
| Gabriel Oliveira Brito |
| Ivan Cardozo Scolforo Moreira |
| Vitor Claudino |

## Jogos

- **Objetos Ocultos** — jogo de encontrar objetos escondidos na cena (WPF).
- **The Last Hunter** — jogo de tiro em 2D (Windows Forms).

## Estrutura do repositório

```
UML-GUI-POO/
├── Documentação/          # Documentação de cada jogo (PDF / DOCX)
│   ├── Documentacao_Objetos_Ocultos.2.0-1.pdf
│   └── Documentacao_The_Last_Hunter_v2.docx
├── Legacy/                # Versões antigas do código-fonte (backup)
│   ├── Jogos Ocultos/
│   └── The Last Hunter/
├── Jogos Ocultos.zip      # Jogo "Objetos Ocultos" (versão final)
└── The Last Hunter.zip    # Jogo "The Last Hunter" (versão final)
```

- **`.zip`** → contêm os **jogos** prontos para extrair e abrir no Visual Studio.
- **`Documentação/`** → contém a **documentação de cada jogo**.
- **`Legacy/`** → apenas as **versões antigas**, mantidas como **backup**. Não é necessário para executar os jogos.

## Como executar

1. Baixe o `.zip` do jogo desejado.
2. **Desbloqueie o `.zip` antes de extrair** (veja a seção abaixo).
3. Extraia o conteúdo.
4. Abra o arquivo `.sln` (ou `.csproj`) no Visual Studio e execute com `F5`.

## ⚠️ Possível problema: arquivos bloqueados pelo Windows

O Windows marca arquivos baixados da internet como "bloqueados" (Mark of the Web). Isso pode causar erros ao compilar ou executar o projeto no Visual Studio.

### Solução — desbloquear o ZIP antes de extrair (o mais confiável)

Se ainda tiver o `.zip` original:

1. Clique com o **botão direito** no arquivo `.zip` → **Propriedades**.
2. Na parte de baixo da janela, marque a opção **Desbloquear**.
3. Clique em **OK**.
4. **Apague a pasta extraída** e **extraia novamente**.

Todos os arquivos saem limpos. ✅
