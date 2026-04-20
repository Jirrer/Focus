import sys, time, os, winsound

def main(timerLength):
    while timerLength > 0:
        time.sleep(1)

        if checkTimer(timerLength): timerLength -= 1

        os.system('cls')

        print(f'{timerLength}s left')

    os.system('clear')

    playAlarm()

def checkTimer(timerLength) -> bool:
    with open('logs.txt', 'r', newline='') as file:
        lines = file.readlines()

        lastMinute = lines[len(lines) - 61:]

        actions = []

        for row in lastMinute:
           actions.append(row.split(' ')[2])

        actionsCount = {}

        for a in actions:
            if a in actionsCount: actionsCount[a] += 1
            else: actionsCount[a] = 1


        return userIsWorking(actionsCount)

def userIsWorking(counts: dict) -> bool:
    highestCount = max(counts, key=counts.get)

    productiveActions = {'Coding', 'Studying'}

    return highestCount in productiveActions

def playAlarm():
    winsound.Beep(1000, 500)
    time.sleep(0.25)

    winsound.Beep(1000, 500)
    time.sleep(0.25)

    winsound.Beep(1000, 500)
    time.sleep(0.25)


if __name__ == "__main__":
    if len(sys.argv) == 2: timer_length = int(sys.argv[1])
    else: timer_length = 3600
    
    main(timer_length)