#include <iostream>
#include <thread>
#include <Windows.h>
#include <chrono>
#include <string>
#include <mutex>
#include <atomic>
#include <deque>
#include <algorithm>

using namespace std;

int IDLE_TIME = 0;
int TIMER_TIME = 0;
int IDLE_THRESHOLD = 90; // 90
int TIMER_THRESHOLD = 1200; // 1200
int GPU_RANGED_0_30 = 1;

PROCESS_INFORMATION pi;

const char* PIPE_NAME = R"(\\.\pipe\GPUUsagePipe)";

std::atomic<int> latestGPUUsage(0);

int getLatestGPUUsage() {
    return latestGPUUsage.load();
}

void updateGPU(int value) {
    if (value <= 1) { --GPU_RANGED_0_30; }
    else { ++GPU_RANGED_0_30; }

    GPU_RANGED_0_30 = max(0, min(30, GPU_RANGED_0_30));
    
}

std::mutex coutMutex;

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
                std::string str(buffer);
                // Trim whitespace
                str.erase(0, str.find_first_not_of(" \t\r\n"));
                str.erase(str.find_last_not_of(" \t\r\n") + 1);
                try {
                    if (!str.empty() && std::all_of(str.begin(), str.end(), ::isdigit)) {
                        int value = stoi(str);
                        latestGPUUsage.store(value);

                        updateGPU(value);

                    } else {
                        lock_guard<mutex> lock(coutMutex);
                    }
                }
                catch (...) {
                    lock_guard<mutex> lock(coutMutex);
                    cout << "Failed to parse GPU usage value: '" << str << "'" << endl;
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
        
        if (idleTime >= 1000) { ++IDLE_TIME;}
        else { IDLE_TIME = 0; }
    }
    
    bool GPU_running = true;

    if (GPU_RANGED_0_30 < 20) {
        GPU_running = false;
    }
    


    if (GPU_running || IDLE_TIME <= IDLE_THRESHOLD) { ++TIMER_TIME; }
    else {TIMER_TIME = 0; }
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

    wchar_t exePath[MAX_PATH];
    GetModuleFileNameW(NULL, exePath, MAX_PATH);

    std::wstring exeDir(exePath);
    exeDir = exeDir.substr(0, exeDir.find_last_of(L"\\/"));
    std::wstring ps1Path = exeDir + L"\\monitorGPU.ps1";

    std::wstring cmd = L"powershell.exe -ExecutionPolicy Bypass -File \"" + ps1Path + L"\"";

    if (!CreateProcessW(nullptr, &cmd[0], nullptr, nullptr, FALSE, 0, nullptr, nullptr, &si, &pi)) {
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

    std::cout.sync_with_stdio(true);
    std::cout << std::unitbuf; // unbuffered output

    while (true) {
        checkIdle();

        int gpuUsage = getLatestGPUUsage();

        {
            lock_guard<mutex> lock(coutMutex);
            cout << "Current GPU Usage: " << gpuUsage << "%" << endl;
            cout << "GPU Count: " << GPU_RANGED_0_30 << endl;
            cout << "Idle Time: " << IDLE_TIME << "s" << endl;
            cout << "Running Time: " << TIMER_TIME << "s\n" << endl;
        }

        this_thread::sleep_for(chrono::seconds(1));
    }

    return 0;
}
