I am currently engaged in the development of an open-source tool designed to capture computer telemetry data. This data encompasses CPU usage, temperature, memory statistics, and disk space metrics, sourced from various origins. Once the data has been successfully collected, it is transmitted to MongoDB for archival purposes, preserving historical records. Subsequently, the processed data is forwarded to Grafana, where it undergoes sophisticated visualization for analytical insights. 

The data payload, which includes CPU temperature/utilization, memory utilization, disk utilization, as well as the host name, IP address, and MAC address, is transmitted to MongoDB as indicated below
![image](https://github.com/user-attachments/assets/1268d5ef-5a50-47fb-81d0-beeac1f1d562)
