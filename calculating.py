




def processesCategories(lines: list):
    categories = {}

    for row in lines:
        if not row: continue

        splitRow = row.split(' ')

        action = splitRow[2]

        date = splitRow[4]
        year = date[:4]

        time = splitRow[5].replace(':', '')
        seconds = float(time)

        if action in categories: categories[action] += 1
        else: categories[action] = 1

    print(categories)


def getDateCode(date):
    return 109


if __name__ == "__main__":
    processesCategories('logs.txt')