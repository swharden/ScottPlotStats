import pathlib
from dataclasses import dataclass
#import requests
import json
import datetime


@dataclass
class Record(object):
    timestamp: str
    package: str
    downloads: int


def readRecords():
    thisFolder = pathlib.Path(__file__).parent
    with open(thisFolder.joinpath("packageStats.csv")) as f:
        lines = f.readlines()[1:]

    records = []
    for line in lines:
        parts = line.strip().split(",")
        records.append(Record(parts[2], parts[5], int(parts[3])))
    print(f"loaded {len(records):,} records")
    return records


def saveRecords(records):
    lines = []
    packageName = records[0].package.lower()
    for record in records:
        lines.append(f'"{record.timestamp}":{record.downloads}')
    thisFolder = pathlib.Path(__file__).parent
    saveFilePath = thisFolder.joinpath(f"logs-{packageName}.json")
    recordsJson = "{" + ",".join(lines) + "}"
    json = "{" + f'"package":"{packageName}","records":{recordsJson}' + "}"
    with open(saveFilePath, 'w') as f:
        f.write(json)


def removeDuplicates(records):
    unique = []
    lastCount = -1
    for record in records:
        if record.downloads != lastCount:
            unique.append(record)
            lastCount = record.downloads
    return unique


def integrateTrends(knownRecords, packageName, months=99):
    #url = "https://nugettrends.com/api/package/history/{packageName}?months={months}"

    with open(fR"C:\Users\scott\Documents\GitHub\Package-Popularity-Tracker\dev\data\nugettrends-{packageName}.json") as f:
        requestText = f.read()

    firstRecordTime = packageRecords[0].timestamp
    firstRecordTime = firstRecordTime.replace("T", " ").split(".")[0]
    firstRecordTime = datetime.datetime.fromisoformat(firstRecordTime)
    for dl in json.loads(requestText)['downloads']:
        timestamp = dl['week']
        downloads = dl['count']
        if downloads is None:
            continue
        timestamp = datetime.datetime.fromisoformat(timestamp)
        if (timestamp < firstRecordTime):
            timestamp = str(timestamp).replace(" ", "T")+".000Z"
            knownRecords.append(Record(timestamp, packageName, downloads))
    return knownRecords


if __name__ == "__main__":
    records = readRecords()
    packages = set([x.package for x in records])
    for package in packages:
        packageRecords = [x for x in records if x.package == package]
        packageRecords = sorted(packageRecords, key=lambda x: x.timestamp)
        packageRecords = removeDuplicates(packageRecords)
        packageRecords = integrateTrends(packageRecords, package)
        packageRecords = sorted(packageRecords, key=lambda x: x.timestamp)
        print(f"{package} has {len(packageRecords)} records")
        saveRecords(packageRecords)
