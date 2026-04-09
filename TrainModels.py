import pandas as pd
import joblib, sys, os
from sklearn.svm import LinearSVC
from sklearn.pipeline import Pipeline
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report
from dotenv import load_dotenv

load_dotenv()

def buildModel(filePath: str):
    df = pd.read_csv(filePath)

    titles = df.iloc[:, 0].tolist()

    labels = df.iloc[:, 1].tolist()

    X_train, X_test, y_train, y_test = train_test_split(
        titles, labels, test_size=0.2, random_state=42, stratify=labels
    )

    model = Pipeline([
        ("tfidf", TfidfVectorizer(ngram_range=(1,2))),
        ("clf", LinearSVC(class_weight="balanced"))
    ])

    model.fit(X_train, y_train)

    predictions = model.predict(X_test)
    print(classification_report(y_test, predictions))

    joblib.dump(model, str(os.getenv('WINDOW_TITLE_CLASSIFIER')))

if __name__ == "__main__": # To-Do: add label verification and normalizer
    if len(sys.argv) != 2:
        print("Failed - Provide Training CSV Data File Path")

        exit()

    buildModel(sys.argv[1])
