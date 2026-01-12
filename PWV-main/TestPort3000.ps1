Write-Host "=========================================="
Write-Host "   PRUEBA DE PUERTO 3000 - ETHER DOMES    "
Write-Host "=========================================="
Write-Host ""
Write-Host "1. Iniciando servidor de prueba en puerto 3000..."

try {
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Any, 3000)
    $listener.Start()
    Write-Host "[OK] EXITO: El puerto 3000 se abrio correctamente en esta PC." -ForegroundColor Green
    Write-Host ""
    Write-Host "=========================================="
    Write-Host "   AHORA VIENE LA PRUEBA REAL (INTERNET)  "
    Write-Host "=========================================="
    Write-Host "1. NO CIERRES ESTA VENTANA."
    Write-Host "2. Entra a esta web: https://www.canyouseeme.org/"
    Write-Host "3. En 'Port to Check' escribe: 3000"
    Write-Host "4. Dale al boton 'Check Port'."
    Write-Host ""
    Write-Host "--> Si dice 'Success': Tu Router esta bien configurado. El problema es el juego."
    Write-Host "--> Si dice 'Error': Tu Router NO esta dejando pasar a nadie. Tienes que revisar el 'Port Forwarding'."
    Write-Host ""
    Write-Host "Esperando conexion... (Presiona Ctrl+C para salir)"

    while ($true) {
        if ($listener.Pending()) {
            $client = $listener.AcceptTcpClient()
            Write-Host "CONEXION RECIBIDA! Alguien logro entrar desde afuera." -ForegroundColor Cyan
            $client.Close()
        }
        Start-Sleep -Milliseconds 100
    }
}
catch {
    Write-Host "[ERROR] No se pudo abrir el puerto 3000." -ForegroundColor Red
    Write-Host "Causa posible: Otro programa (quizas el juego?) ya lo esta usando."
    Write-Host "Cierra el juego/Unity y prueba de nuevo."
    Write-Host $_
}
finally {
    if ($listener) { $listener.Stop() }
}
