import time, pywinctl, joblib
from datetime import datetime
from enum import Enum
from idle_time import IdleMonitor
import calculating


# To-Do: model can learn from other assemenst (if final outcome is coding then train that checkWorkingWindow to be coding)
# To-Do: maybe move assessments to another file

workingWindowClassifier = joblib.load('classifiers\\window_title_classifier.joblib') 

class FailedWritingNewLogFile(Exception): pass
class FailedParsingLogFile(Exception): pass

class Action(Enum):
    CODING = 'Coding'
    STUDYING = 'Studying'
    AFK = 'AFK'
    PLAYBACK = 'Playback'
    OTHER = 'Other'

STATUS = Action.OTHER

def updateStatus(newStatus: Action):
    global STATUS

    STATUS = newStatus

def normalize(content: str) -> str:
    content = content.lower()

    return content

def main():
    assessments = [ # Order of least resource intensive
        idleTime,
        checkWorkingWindow,
        takeScreenShot
    ]

    loopTime = 1

    while (2):
        for a in assessments:
            if a() == True: break # Confident in decisgn

        log(f'User is {STATUS.value}', datetime.now())

        time.sleep(loopTime)

def idleTime() -> bool:
    monitor = IdleMonitor.get_monitor()

    if monitor.get_idle_time() > 30.00: updateStatus(Action.AFK)
    elif STATUS == Action.AFK: updateStatus(Action.OTHER)

    return False
    
# To-Do: think of ways to hold logs in memory so i am not constally using disk
def log(action: str, timestamp: datetime):
    try:
        with open('logs.txt', 'a+', newline='') as file: 
            file.write(f'{action} - {timestamp}\n')

            file.seek(0)

            lines = file.readlines()

        if len(lines) >= 1000:
            refactorLogFile(lines)

    except Exception as e:
        print(f'Failed log: {e}')

def refactorLogFile(lines: list):
    try:
        linesToRemove = lines[:500]
        linesToKeep = lines[500:]

        calculating.processesCategories(linesToRemove)

    except Exception as e:
        FailedParsingLogFile

    try:
        with open('logs.txt', 'w', newline='') as file:
            for line in linesToKeep:
                file.write(line)
    
    except Exception as e:
        raise FailedWritingNewLogFile


def checkWorkingWindow() -> bool:
    activeWindowTitle = pywinctl.getActiveWindowTitle()

    result = workingWindowClassifier.predict([activeWindowTitle])

    if STATUS == Action.AFK and result == Action.PLAYBACK.value:    
        updateStatus(Action.PLAYBACK)
        return False
    
    elif STATUS == Action.AFK:
        return True

    else:
        match (result[0]):
            case 'other': updateStatus(Action.OTHER); return False
            case 'coding': updateStatus(Action.CODING); return True
            case _: updateStatus(Action.OTHER); return False

def takeScreenShot() -> bool:
    
    return False


if __name__ == "__main__":
    main()