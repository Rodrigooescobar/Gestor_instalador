using System.Diagnostics;
using System.Text;

namespace InstaladorProgramas;

internal sealed class InstallerForm : Form
{
    private readonly CheckBox _acrobat = new() { Text = "Adobe Acrobat Reader", AutoSize = true };
    private readonly CheckBox _openVpn = new() { Text = "OpenVPN", AutoSize = true };
    private readonly CheckBox _chrome = new() { Text = "Google Chrome", AutoSize = true };
    private readonly CheckBox _winRar = new() { Text = "WinRAR", AutoSize = true };
    private readonly CheckBox _office = new() { Text = "Office 2016", AutoSize = true };
    private readonly Button _selectAll = new() { Text = "Seleccionar todos", AutoSize = true };
    private readonly Button _clearAll = new() { Text = "Limpiar selección", AutoSize = true };
    private readonly Button _install = new() { Text = "Instalar seleccionados", AutoSize = true };
    private readonly TextBox _status = new()
    {
        Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
        Dock = DockStyle.Fill, WordWrap = true
    };

    private string BaseDirectory => AppContext.BaseDirectory;

    public InstallerForm()
    {
        Text = "Instalador de programas";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(560, 420);
        Size = new Size(680, 520);

        var intro = new Label
        {
            Text = "Seleccioná los programas que querés instalar. Los instaladores deben estar en la carpeta programas, junto a este programa.",
            AutoSize = true, MaximumSize = new Size(610, 0), Padding = new Padding(0, 0, 0, 12)
        };
        var checks = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, Padding = new Padding(0, 8, 0, 8)
        };
        checks.Controls.AddRange([_acrobat, _openVpn, _chrome, _winRar, _office]);
        var selectionButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = true, WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 0, 0, 8)
        };
        selectionButtons.Controls.AddRange([_selectAll, _clearAll]);
        _selectAll.Click += (_, _) => SetAllChecked(true);
        _clearAll.Click += (_, _) => SetAllChecked(false);
        _install.Click += async (_, _) => await InstallSelectedAsync();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(intro, 0, 0);
        layout.Controls.Add(checks, 0, 1);
        layout.Controls.Add(selectionButtons, 0, 2);
        layout.Controls.Add(_install, 0, 3);
        layout.Controls.Add(_status, 0, 4);
        Controls.Add(layout);
        AppendStatus("Listo. Elegí uno o más programas.");
    }

    private async Task InstallSelectedAsync()
    {
        var jobs = new List<(string Name, Func<Task<int>> Run)>();
        if (_acrobat.Checked) jobs.Add(("Adobe Acrobat Reader", InstallAcrobatAsync));
        if (_openVpn.Checked) jobs.Add(("OpenVPN", InstallOpenVpnAsync));
        if (_chrome.Checked) jobs.Add(("Google Chrome", InstallChromeAsync));
        if (_winRar.Checked) jobs.Add(("WinRAR", InstallWinRarAsync));
        if (_office.Checked) jobs.Add(("Office 2016", InstallOfficeAsync));

        if (jobs.Count == 0)
        {
            MessageBox.Show(this, "Seleccioná al menos un programa.", "Instalador", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _install.Enabled = false;
        SetChecksEnabled(false);
        var results = new List<string>();
        try
        {
            foreach (var job in jobs)
            {
                AppendStatus($"Iniciando {job.Name}...");
                try
                {
                    var exitCode = await job.Run();
                    if (exitCode == 0)
                    {
                        results.Add($"{job.Name}: instalado correctamente");
                        AppendStatus($"{job.Name}: finalizó correctamente.");
                    }
                    else
                    {
                        results.Add($"{job.Name}: falló (código {exitCode})");
                        AppendStatus($"{job.Name}: falló con código {exitCode}.");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"{job.Name}: falló ({ex.Message})");
                    AppendStatus($"{job.Name}: error: {ex.Message}");
                }
            }
        }
        finally
        {
            _install.Enabled = true;
            SetChecksEnabled(true);
        }

        MessageBox.Show(this, string.Join(Environment.NewLine, results), "Resultado de las instalaciones",
            MessageBoxButtons.OK, results.Any(r => r.Contains(": falló", StringComparison.Ordinal)) ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
    }

    private Task<int> InstallAcrobatAsync() => RunInstallerAsync(
        Path.Combine(BaseDirectory, "programas", "Acrobat", "Setup.exe"), "/sPB");

    private Task<int> InstallOpenVpnAsync() => RunInstallerAsync(
        "msiexec.exe", $"/i \"{Path.Combine(BaseDirectory, "programas", "OpenVPN-2.5.7-I602-amd64.msi")}\" /norestart /passive");

    private Task<int> InstallChromeAsync()
    {
        var candidates = new[]
        {
            Path.Combine(BaseDirectory, "programas", "ChromeSetup.exe"),
            Path.Combine(BaseDirectory, "programas", "p", "ChromeSetup.exe")
        };
        var setup = candidates.FirstOrDefault(File.Exists) ?? candidates[0];
        return RunInstallerAsync(setup, "/silent");
    }

    private Task<int> InstallWinRarAsync() => RunInstallerAsync(
        Path.Combine(BaseDirectory, "programas", "WinRAR.exe"), "/s");

    private async Task<int> InstallOfficeAsync()
    {
        var iso = Path.Combine(BaseDirectory, "programas", "Office2016.iso");
        var config = Path.Combine(BaseDirectory, "programas", "config.xml");
        if (!File.Exists(iso)) throw new FileNotFoundException("No se encontró programas\\Office2016.iso.", iso);
        if (!File.Exists(config)) throw new FileNotFoundException("No se encontró programas\\config.xml.", config);

        // La ISO se monta y desmonta en PowerShell, siguiendo el procedimiento indicado.
        var script = "$ErrorActionPreference='Stop'; " +
            "$iso=(Resolve-Path 'programas\\Office2016.iso').Path; " +
            "$config=(Resolve-Path 'programas\\config.xml').Path; " +
            "$img=Mount-DiskImage -ImagePath $iso -PassThru; " +
            "try { $vol=$img | Get-Volume; $letra=$vol.DriveLetter; " +
            "if (-not $letra) { throw 'No se encontro la letra de la ISO' }; " +
            "$setup=$letra+':\\setup.exe'; if (-not (Test-Path $setup)) { throw 'No se encontro setup.exe' }; " +
            "$proc=Start-Process -FilePath $setup -ArgumentList ('/config \"'+$config+'\"') -Wait -PassThru; " +
            "if ($proc.ExitCode -ne 0) { throw ('Codigo de salida: '+$proc.ExitCode) } " +
            "} finally { Dismount-DiskImage -ImagePath $iso }";
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
        return await RunInstallerAsync("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {encoded}");
    }

    private async Task<int> RunInstallerAsync(string fileName, string arguments)
    {
        if (!File.Exists(fileName) && !string.Equals(fileName, "msiexec.exe", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(fileName, "powershell.exe", StringComparison.OrdinalIgnoreCase))
            throw new FileNotFoundException($"No se encontró el instalador: {Path.GetRelativePath(BaseDirectory, fileName)}", fileName);

        var start = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = BaseDirectory,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = new Process { StartInfo = start };
        if (!process.Start()) throw new InvalidOperationException("No se pudo iniciar el proceso.");
        await process.WaitForExitAsync();
        return process.ExitCode;
    }

    private void AppendStatus(string message)
    {
        _status.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private void SetChecksEnabled(bool enabled)
    {
        _acrobat.Enabled = enabled;
        _openVpn.Enabled = enabled;
        _chrome.Enabled = enabled;
        _winRar.Enabled = enabled;
        _office.Enabled = enabled;
        _selectAll.Enabled = enabled;
        _clearAll.Enabled = enabled;
    }

    private void SetAllChecked(bool isChecked)
    {
        _acrobat.Checked = isChecked;
        _openVpn.Checked = isChecked;
        _chrome.Checked = isChecked;
        _winRar.Checked = isChecked;
        _office.Checked = isChecked;
    }
}
