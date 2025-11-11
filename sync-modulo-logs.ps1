# Script para sincronizar o módulo "Logs de Notificações" no banco de produção
# Execute este script após fazer o deploy do backend

$TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE4NjllNmQzLTU1ZTMtNGZmYy1iOTg1LTI4ZDEwOGYxOTczNiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBZG1pbmlzdHJhZG9yIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiYWRtaW5AZW1wcmVzYWRlbW8uY29tLmJyIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJUZW5hbnRJZCI6IjI5Y2Q0M2VjLTYwODctNDcwMS1hMmU4LWU5NTAzZDJjODE2YSIsIkVtcHJlc2FDbGllbnRlSWQiOiIyOWNkNDNlYy02MDg3LTQ3MDEtYTJlOC1lOTUwM2QyYzgxNmEiLCJqdGkiOiIxZWExMTk5NC05MTQ4LTQzMjctODg4Zi1jYmU1ZDQxMTM5ZWEiLCJleHAiOjE3NjIyODY4MzgsImlzcyI6IkNvYnJpby5BUEkiLCJhdWQiOiJDb2JyaW8uV2ViIn0.ScmowDx8rN6p3qkIIFIE9IjNArG3WZTsgdLWeM8Dl0k"

Write-Host "=== Sincronizando Módulos e Permissões ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Este script irá:" -ForegroundColor Yellow
Write-Host "1. Adicionar o módulo 'Logs de Notificações' ao sistema" -ForegroundColor White
Write-Host "2. Criar permissões padrão para Admin (acesso total)" -ForegroundColor White
Write-Host "3. Manter permissões existentes intactas" -ForegroundColor White
Write-Host ""

$url = "https://cobrio.com.br/api/Admin/sync-permissions"

try {
    Write-Host "Enviando requisição para: $url" -ForegroundColor Gray

    $response = Invoke-RestMethod -Uri $url -Method Post -Headers @{
        "Authorization" = "Bearer $TOKEN"
        "Content-Type" = "application/json"
    }

    Write-Host ""
    Write-Host "✅ SUCESSO!" -ForegroundColor Green
    Write-Host $response.message -ForegroundColor Green
    Write-Host ""
    Write-Host "Agora o módulo 'Logs de Notificações' está disponível em Permissões!" -ForegroundColor Cyan
}
catch {
    Write-Host ""
    Write-Host "❌ ERRO ao sincronizar" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""

    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "⚠️ Token expirado! Faça login novamente e atualize a variável `$TOKEN neste script." -ForegroundColor Yellow
    }
}
