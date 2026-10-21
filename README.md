# Instalador de programas silencioso

Proyecto echeo con WinForms .NET 8. Al iniciar el programa se solicita elevación, del cual solo se pedira 1 vez para multiples programas, por lo que Windows muestra un único UAC. Los procesos de instalación se ejecutan desde esa aplicación elevada.

## Archivos de instalación

Los instaladores se deben ubicar en la siguiente ruta, manteniendo sus nombres:

```text
programas/
├── Acrobat/Setup.exe
├── OpenVPN-2.5.7-I602-amd64.msi
├── ChromeSetup.exe
├── WinRAR.exe
├── Office2016.iso
└── config.xml
```

El `config.xml` incluido corresponde a Office 2016. La extensión correcta para el contenido XML es `.xml`, donde se indica la configuracion para la instalacion silenciosa.

## Comandos usados

- Acrobat: `Setup.exe /sPB`
- OpenVPN: `msiexec /i "OpenVPN-2.5.7-I602-amd64.msi" /norestart /passive`
- Chrome: `ChromeSetup.exe /silent`
- WinRAR: `WinRAR.exe /s`
- Office 2016: monta la ISO, ejecuta `setup.exe /config "...\config.xml"`, espera el resultado y desmonta la ISO.

Las instalaciones se procesan en orden, aunque una falle. Al terminar aparece un resumen con el resultado de cada una.

## Como usar

Abrí `InstaladorProgramas.csproj` con Visual Studio 2022 (carga de trabajo .NET para escritorio) o compilá con el SDK .NET 8 instalado en Windows. El proyecto está configurado para producir una aplicación autocontenida x64:

```powershell
dotnet build -c Release
```

El ejecutable compilado se genera en `bin\Release\net8.0-windows\win-x64\`. El `config.xml` se copia automáticamente a `bin\Release\net8.0-windows\win-x64\programas\`. Copiá los instaladores en las ubicaciones correspondientes dentro de esa carpeta `programas` antes de usarlo. El proyecto no distribuye los instaladores comerciales.

## Agrega tu programa silencioso
Crea un bat con el nombre del exe y se debe agregar /? o /h

![imagen logo de la carrera](/imagenes/Screenshot_1.jpg "UTN Tecnicatura universitaria en programacionar")