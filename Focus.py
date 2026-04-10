import time, pywinctl, joblib
from datetime import datetime
from enum import Enum
from idle_time import IdleMonitor


# To-Do: model can learn from other assemenst (if final outcome is coding then train that checkWorkingWindow to be coding)
# To-Do: maybe move assessments to another file

workingWindowClassifier = joblib.load('classifiers\\window_title_classifier.joblib') 

class Action(Enum):
    CODING = 'Coding'
    STUDYING = 'Studying'
    AFK = 'AFK'
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

def log(action: str, timestamp: datetime):
    try:
        with open('logs.txt', 'a', newline='') as file: 
            file.write(f'{action} - {timestamp}\n')

    except Exception as e:
        print(f'Failed log: {e}')

def checkWorkingWindow() -> bool:
    activeWindowTitle = pywinctl.getActiveWindowTitle()

    result = workingWindowClassifier.predict([activeWindowTitle])

    match (result[0]):
        case 'other': updateStatus(Action.OTHER); return False
        case 'coding': updateStatus(Action.CODING); return True
        case _: updateStatus(Action.OTHER); return False

def takeScreenShot() -> bool:
    
    return False

def idleTime() -> bool:
    monitor = IdleMonitor.get_monitor()

    if monitor.get_idle_time() < 30.00: return False
    
    updateStatus(Action.AFK)
    
    return True


if __name__ == "__main__":
    main()