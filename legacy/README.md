## Installer
[⬇️ Download Installer]()

## Defintion
Focus is a lightweight Windows desktop application designed to help users manage their time effectively. Unlike a traditional timer, Focus is “smart” — it runs for a set duration before reminding you to take a break, but it automatically pauses if it detects that the user has been inactive for too long. This ensures that break reminders are meaningful and only occur when you’re actually working.

Focus also monitors GPU usage to determine if inactivity is due to passive activities, such as watching videos or streaming media. In such cases, the timer continues running, ensuring that planned break intervals remain accurate.

## How to use it
To use Focus, set your desired work session duration in the app’s main interface and press “Start.” The timer will begin counting down. If you are inactive for an extended period, Focus will pause automatically unless it detects ongoing GPU activity (e.g., video playback). When the timer reaches zero, a notification will prompt you to take a break.

## Tech Stack
| Tech | Use|
|-|-|
|C#|GUI
|C++|Backend Logic
|PoweShell|Resource Monitoring

## Prerequisites
* Ensure PowerShell scripts can be run on machine

## How it works
Focus’s frontend is written in C# using the WPF (Windows Presentation Foundation) framework. The UI is designed to be minimal and distraction-free, with quick-access controls for starting, pausing, and resetting the timer.

Focus uses Windows system hooks to detect keyboard and mouse input for determining user activity. If no input is detected for longer than the configured idle threshold, the timer will pause automatically — unless GPU usage suggests the user is engaged in passive activity.

If GPU usage is above the configured percentage threshold, Focus assumes the user is engaged (e.g., watching a video) and keeps the timer running even without direct input.

When the timer reaches zero, an alarm timer is played, prompting the user to take a break.
