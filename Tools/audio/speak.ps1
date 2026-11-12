# Turns lines of text into WAV files using the voices built into Windows (works offline).
# Called by generate_speech.py; you don't need to run this yourself.
#   speak.ps1 -List lines.tsv -OutDir <folder>
# lines.tsv: one line per clip:  language <TAB> file name <TAB> text     (language: es or en)
param([string]$List, [string]$OutDir)

Add-Type -AssemblyName System.Runtime.WindowsRuntime
$null = [Windows.Media.SpeechSynthesis.SpeechSynthesizer, Windows.Media.SpeechSynthesis, ContentType = WindowsRuntime]
$null = [Windows.Storage.Streams.DataReader, Windows.Storage.Streams, ContentType = WindowsRuntime]

$asTask = ([System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object {
        $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType.Name -eq 'IAsyncOperation`1' })[0]
function Await($operation, [Type]$type) {
    $task = $asTask.MakeGenericMethod($type).Invoke($null, @($operation))
    $task.Wait(-1) | Out-Null
    return $task.Result
}

$voices = [Windows.Media.SpeechSynthesis.SpeechSynthesizer]::AllVoices
$pick = @{
    'es' = ($voices | Where-Object { $_.Language -like 'es-ES*' } | Select-Object -First 1)
    'en' = ($voices | Where-Object { $_.Language -like 'en-*' -and $_.Gender -eq 'Female' } | Select-Object -First 1)
}
if (-not $pick['es']) { $pick['es'] = ($voices | Where-Object { $_.Language -like 'es*' } | Select-Object -First 1) }
foreach ($k in 'es', 'en') {
    if ($pick[$k]) { Write-Output "voice $k = $($pick[$k].DisplayName)" } else { Write-Output "voice $k = MISSING" }
}

$synth = New-Object Windows.Media.SpeechSynthesis.SpeechSynthesizer
$made = 0; $skipped = 0
foreach ($line in [IO.File]::ReadAllLines($List, [Text.Encoding]::UTF8)) {
    $parts = $line.Split("`t")
    if ($parts.Count -lt 3) { continue }
    $lang, $name, $text = $parts[0], $parts[1], $parts[2]
    if (-not $pick[$lang]) { $skipped++; continue }
    $dir = Join-Path $OutDir $lang
    New-Item -ItemType Directory -Force $dir | Out-Null
    $path = Join-Path $dir ($name + '.wav')
    if (Test-Path $path) { continue }
    $synth.Voice = $pick[$lang]
    $synth.Options.SpeakingRate = 0.9
    $stream = Await ($synth.SynthesizeTextToStreamAsync($text)) ([Windows.Media.SpeechSynthesis.SpeechSynthesisStream])
    $reader = New-Object Windows.Storage.Streams.DataReader($stream.GetInputStreamAt(0))
    $null = Await ($reader.LoadAsync([uint32]$stream.Size)) ([uint32])
    $bytes = New-Object byte[] ([int]$stream.Size)
    $reader.ReadBytes($bytes)
    [IO.File]::WriteAllBytes($path, $bytes)
    $made++
}
Write-Output "made $made clips, skipped $skipped (no voice installed for that language)"
