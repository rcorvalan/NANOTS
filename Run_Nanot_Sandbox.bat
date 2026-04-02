@echo off
echo ==============================================
echo Iniciando Servidor NANOT Evolution Sandbox...
echo ==============================================
cd %~dp0\app
call npm install
call npm run dev
pause
