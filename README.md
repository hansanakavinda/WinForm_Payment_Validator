process flow

user uploads excel file
reads the excel file and display data - use EPPlus
click validate payment button
for each row
    get payment slip image from google drive
        authenticate google drive api with user details
    preprocess payment image
    read image text data - use tesseract ocr tool
        extract data into readable format
    compare extracted data and excel columns
    if values equal
        mark as valild
    else
        mark as invalid
display valid and invalid payment count
display all the records clearly seperating valild and invalid ones with a new column 
    green for valild column red for invalild coumns


tech stack
.NET 8
EPPlus 8.3.0
Tesseract 5.2.0
Google.Apis.Drive.v3
OAuth 2.0
