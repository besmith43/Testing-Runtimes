#!/usr/bin/env python3

import psutil
import os
import sys
import csv
from datetime import date

def start():
    macAddress = getMac()

    if macAddress == None:
        print("Failed to get MAC Address")
        return

    hostname = os.uname().nodename

    operating_system = sys.platform

    saveCSV(hostname, macAddress, operating_system)

def getMac():
    net = None

    net_adapters = psutil.net_if_addrs()

    for net_adapter in net_adapters:
        if net_adapter == 'en0':
            net = psutil.net_if_addrs()[net_adapter]
            return net[1].address
        elif net_adapter == 'Ethernet 1':
            net = psutil.net_if_addrs()[net_adapter]
            return net[1].address
        elif net_adapter == 'Ethernet 2':
            net = psutil.net_if_addrs()[net_adapter]
            return net[1].address
        elif net_adapter == 'Ethernet 3':
            net = psutil.net_if_addrs()[net_adapter]
            return net[1].address


def saveCSV(host, address, op):
    f = open(os.getcwd() + '/' + str(date.today().month) + str(date.today().day) + str(date.today().year) + '-' + host + '.csv', 'w')

    writer = csv.writer(f)

    writer.writerow(['hostname','mac address', 'operating system'])

    writer.writerow([host, address, op])

    f.close()

if __name__ == "__main__":
    start()
