# Development guide

[Back to README](../README.md)

## Build and test

Install the .NET 10 SDK and Node.js. From the repository root, prepare frontend assets:

```bash
npm ci --prefix src/Aiursoft.Template/wwwroot
npm run build --prefix src/Aiursoft.Template/wwwroot
```

Restore, build and test:

```bash
dotnet restore Aiursoft.Template.sln
dotnet build Aiursoft.Template.sln --no-restore
dotnet test Aiursoft.Template.sln --no-restore
```

Run the existing static checks:

```bash
./lint.sh
```

The checked-in NuGet and npm configuration is for external contributors. Company CI selects its internal build sources separately. NugetNinja maintains shared files through [ninja.yaml](../ninja.yaml).

## README structure

Web applications keep the same base sections, common prose and command layout. Replace only project parameters and the declared product-specific content. The ordered section catalog is:

1. `Overview`
2. `CLI & SDK` (optional client capability)
3. `Try`
4. `Install with Docker`
5. `Run manually`
6. `Run in Microsoft Visual Studio`
7. `Run in Docker`
8. `Usage`
9. `Configuration`
10. `Documentation`
11. `How to contribute`
12. `License`

Only optional capability sections may be omitted. Keep the project name, badges and short description above the sections. Put product features in Overview, client installation and SDK availability in CLI & SDK, task examples and permissions in Usage, and reference links in Documentation. Product topics use level-three or deeper headings inside these sections.

Client capabilities extend the same Web README rather than introducing a separate format. New capability headings must first be added to this catalog before consumers use them. Keep existing section links working when moving topics. Document only published packages and verified commands; source-only clients use build instructions. README bodies remain project-specific and are not copied wholesale by NugetNinja.

Keep the common prose as well as the section order identical for applications using the same template. Replace project names, package and image names, source paths and URLs with the current project's values. Product-specific descriptions, examples and configuration belong in Overview, Usage and Configuration. Client capabilities use the fixed CLI & SDK section; they do not require another complete Web README format.

## Projects using Aiursoft Template

* [Stathub](https://github.com/aiursoftweb/stathub)
* [MarkToHtml](https://github.com/aiursoftweb/marktohtml)
* [MusicTools](https://github.com/aiursoftweb/musictools)
* [AnduinOS Home](https://github.com/aiursoftweb/AnduinOS-Home)
* [Manhours](https://github.com/aiursoftweb/manhours)
* [Tracer](https://github.com/aiursoftweb/tracer)
* [Warp](https://github.com/aiursoftweb/warp)
* [AiurDrive](https://github.com/aiursoftweb/aiurdrive)
* [EmployeeCenter](https://github.com/aiursoftweb/employeecenter)
* [Git Mirror Server](https://github.com/aiursoftweb/gitmirrorserver)
* [CppRunner](https://github.com/aiursoftweb/cpprunner)
* [Ollama Gateway](https://github.com/aiursoftweb/ollamagateway)
* [Polls](https://github.com/aiursoftweb/polls)
* [WeChatExam](https://github.com/aiursoftweb/wechatexam)
* [MusicExam](https://github.com/aiursoftweb/musicexam)
* [CorpHome](https://github.com/aiursoftweb/corphome)
* [Events Recorder](https://github.com/aiursoftweb/eventsrecorder)
* [Translate](https://github.com/aiursoftweb/translate)
* [Apkg](https://github.com/aiursoftweb/apkg)
* [Nupkg](https://github.com/aiursoftweb/nupkg)
* [Nopkg](https://github.com/aiursoftweb/nopkg)
* [HowToCook Viewer](https://github.com/aiursoftweb/howtocookviewer)
* [Kanban](https://github.com/aiursoftweb/kanban)
* [MoongladeV2](https://github.com/aiursoftweb/moongladev2)
* [DocsViewer](https://github.com/aiursoftweb/docsviewer)
* [S360](https://github.com/aiursoftweb/s360)
* [AlertCenter](https://github.com/aiursoftweb/alertcenter)
* [ChessServer](https://github.com/aiursoftweb/chessserver)
