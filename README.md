This is HTML to PDF.Net + Vue Web Service.

The service accept an HTML file from a web client, convert it to PDF using Puppeteer Sharp, and return it to the client.
The user can send files for conversion by uploading them from computer. 
The Vue client display the list of files sent to conversion (keeping it in memory). 
Each list item contain filename, option to convert the file, option to download result, option to remove item.

Client: Vue.js

Server: .NET

<img width="600" alt="Screenshot 2023-12-17 174508" src="https://github.com/dmitriimadden/HTML-TO-PDF-NET/assets/59518264/f98ad2a0-754f-464e-9aa9-54d63b0f84a7">

In order to make this project scalable, local storage need to be changed to a cloud storage like firebase-storage.

When debagging make sure both backend and client side running: 

<img width="600" alt="Screenshot 2023-12-17 021621" src="https://github.com/dmitriimadden/HTML-TO-PDF-NET/assets/59518264/c7cd0ccb-a0a1-4496-8467-18a28d68a00a">
