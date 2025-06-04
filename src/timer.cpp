#include <iostream>
#include <thread>
#include <Windows.h>
#include <chrono>
#include <string>
#include <mutex>
#include <atomic>

using namespace std;

int IDLE_TIME = 0;
int TIMER_TIME = 0;
int IDLE_THRESHOLD = 300;
int TIMER_THRESHOLD = 1200;

atomic<int> latestGPUUsage(0);
mutex coutMutex;

PROCESS_INFORMATION pi;

const char* PIPE_NAME = R"(\\.\pipe\GPUUsagePipe)";

void checkGPUUsage() {
    while (true) {
        HANDLE hPipe = CreateNamedPipeA(
            PIPE_NAME,
            PIPE_ACCESS_INBOUND,  // server reads from pipe
            PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_WAIT,
            1,
            0,
            0,
            0,
            NULL);

        if (hPipe == INVALID_HANDLE_VALUE) {
            lock_guard<mutex> lock(coutMutex);
            cout << "Failed to create pipe: " << GetLastError() << endl;
            this_thread::sleep_for(chrono::seconds(1));
            continue;
        }

        BOOL connected = ConnectNamedPipe(hPipe, NULL) ?
                         TRUE : (GetLastError() == ERROR_PIPE_CONNECTED);

        if (connected) {
            char buffer[128];
            DWORD bytesRead;
            while (ReadFile(hPipe, buffer, sizeof(buffer) - 1, &bytesRead, NULL) && bytesRead != 0) {
                buffer[bytesRead] = '\0'; // null terminate
                try {
                    int value = stoi(buffer);
                    latestGPUUsage.store(value);

                    lock_guard<mutex> lock(coutMutex);
                }
                catch (...) {
                    lock_guard<mutex> lock(coutMutex);
                }
            }
        } else {
            lock_guard<mutex> lock(coutMutex);
            cout << "Failed to connect to client: " << GetLastError() << endl;
        }
        DisconnectNamedPipe(hPipe);
        CloseHandle(hPipe);
    }
}

void checkIdle() {
    LASTINPUTINFO lii = {};
    lii.cbSize = sizeof(LASTINPUTINFO);

    if (GetLastInputInfo(&lii)) {
        DWORD currentTime = GetTickCount();
        DWORD idleTime = currentTime - lii.dwTime;

        if (idleTime >= 1000) { ++IDLE_TIME; }
        else { IDLE_TIME = 0; }

        ++TIMER_TIME;
    }
}

void resetTimer() {
    TIMER_TIME = 0;
}

void playTimer() {
    Beep(900, 300);
    Beep(900, 300);
    cout << "----------------- Timer Done -----------------" << endl;
    cout << "\n restart timer" << endl;

    string temp;
    getline(cin, temp);
    resetTimer();
}

void instatiateMonitorGPU() {
    STARTUPINFOW si = { sizeof(si) };
    PROCESS_INFORMATION piLocal = {};

    // Use wide string for the command
    wchar_t cmd[] = L"powershell.exe -ExecutionPolicy Bypass -File monitorGPU.ps1";

    if (!CreateProcessW(nullptr, cmd, nullptr, nullptr, FALSE, 0, nullptr, nullptr, &si, &pi)) {
        std::wcerr << L"Failed to start PowerShell script." << std::endl;
    }
}

BOOL WINAPI ConsoleHandler(DWORD signal) {
    if (signal == CTRL_C_EVENT || signal == CTRL_CLOSE_EVENT) {
        if (pi.hProcess && pi.hProcess != INVALID_HANDLE_VALUE) {
            TerminateProcess(pi.hProcess, 0);
            CloseHandle(pi.hProcess);
            pi.hProcess = nullptr;
        }
        if (pi.hThread && pi.hThread != INVALID_HANDLE_VALUE) {
            CloseHandle(pi.hThread);
            pi.hThread = nullptr;
        }
        std::cout << "GPU Monitor ended" << std::endl;
        ExitProcess(0); 
    }
    return FALSE; 
}

int main() {
    cout << "Running Timer cpp File" << endl;

    instatiateMonitorGPU();

    SetConsoleCtrlHandler(ConsoleHandler, TRUE);

    thread pipeThread(checkGPUUsage);
    pipeThread.detach();

    // Example main loop to use latestGPUUsage
    while (true) {
        checkIdle();

        int gpuUsage = latestGPUUsage.load();

        {
            lock_guard<mutex> lock(coutMutex);
            cout << "Current GPU Usage: " << gpuUsage << "%" << endl;
            cout << "Idle Time: " << IDLE_TIME << "s" << endl;
            cout << "Running Time: " << TIMER_TIME << "s\n" << endl;
        }

        this_thread::sleep_for(chrono::seconds(1));
    }

    return 0;
}
