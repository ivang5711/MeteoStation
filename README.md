# MeteoStationApp v2.0

![Static Badge](https://img.shields.io/badge/dotnet_Framework-%204.7.2-blue) ![Static Badge](https://img.shields.io/badge/platform-Windows-green) <img src="https://img.shields.io/badge/Maintained%3F-yes-green.svg" alt="maintained" height="20px">

<img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="c#" height="28px">
<img src="https://img.shields.io/badge/.NET Framework-5C2D91?style=for-the-badge&logo=.net&logoColor=white" alt=".NET" height="28px"> 
<img src="https://img.shields.io/badge/winforms-8c0a90?style=for-the-badge&logo=.net&logoColor=white" alt=".NET" height="28px"> 
<img src="https://img.shields.io/badge/SQLite-05a5f5?style=for-the-badge&logo=c-sharp&logoColor=white" alt="c#" height="28px">


## Start here

The "Meteo Station" app is designed to work on Windows with Arduino based MeteoStation.\
It collects meteo data from a COM port and stores the data on disk in a database.\
The app also shows the most recent readings and can display historical data via customizable chart.

### How to use the app

0. Prerequisites:
    >You need to have Arduino based meteo station connected to your pc via usb or bluetooth.\
    >It should send data to a COM port.
    >
    > <img src="meteoStation.jpg" alt="meteoStation.jpg" height="300px"/>
    >
    >You can find out the port number in Windows Device manages, or try all the ports one by one.
    >
    > <img src="deviceManager.png" alt="deviceManager.png" height="220px"/>

1. Run the MeteoStation.exe
    > to run the app you need to execure MeteoStation.exe file

2. Select a COM port from the drop down list
    ><img src="uiCom.png" alt="uiCom.png" height="300px"/>

3. Press "Start data collection" button

    > If the port is correct and the data collection works correct the status change to "Collecting Data".\
    >Otherwise an error notification with error description will pop up.
    >
    > <img src="uiStart.png" alt="uiStart.png" height="300px"/>

4. To stop data collection press Stop button.
    > <img src="uiStop.png" alt="uiStop.png" height="300px"/>
5. Change Scale

    > You can choose chart scale by picking a proper radio button.\
    By default the chart scale equals 1 hour.
    >
    ><img src="uiScale.png" alt="uiScale.png" height="300px"/>
