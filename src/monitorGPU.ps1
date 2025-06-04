function getGPUPercent {
    $GpuUseTotal = ((Get-Counter "\GPU Engine(*engtype_3D)\Utilization Percentage").CounterSamples | 
                    Where-Object { $_.CookedValue -ne $null } | 
                    Measure-Object -Property CookedValue -Sum).Sum


    [int]$int_output = [math]::Round($GpuUseTotal)
    
    return $int_output
}


$pipeName = "GPUUsagePipe"

$pipe = New-Object System.IO.Pipes.NamedPipeClientStream(".", $pipeName, [System.IO.Pipes.PipeDirection]::Out)
$pipe.Connect(5000) # 5-second timeout

$writer = New-Object System.IO.StreamWriter($pipe)
$writer.AutoFlush = $true

try {
    while ($true) {
        $data = getGPUPercent
        $writer.WriteLine($data)
        Start-Sleep -Seconds 1
    }
}
finally {
    $writer.Close()
    $pipe.Close()
}