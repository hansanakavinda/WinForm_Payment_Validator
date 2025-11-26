# Payment Validator

A Windows Forms application that automates payment validation by comparing Excel data with payment slip images stored on Google Drive using OCR technology.

## 🎯 Overview

This application helps streamline payment verification by automatically extracting text from payment slip images and comparing it against Excel records. It clearly identifies valid and invalid payments with visual indicators.

## ✨ Features

- 📊 **Excel File Processing**: Upload and display Excel files containing payment records
- 🔍 **Automated OCR**: Extract text data from payment slip images using Tesseract OCR
- ☁️ **Google Drive Integration**: Securely fetch payment slips from Google Drive links
- ✅ **Smart Validation**: Compare extracted OCR data with Excel columns automatically
- 📈 **Visual Results**: Color-coded validation results (Green = Valid, Red = Invalid)
- 📊 **Summary Statistics**: Display count of valid and invalid payments

## 🔧 Tech Stack

- **.NET 8**: Modern cross-platform framework
- **EPPlus 8.3.0**: Excel file reading and manipulation
- **Tesseract 5.2.0**: OCR engine for text extraction from images

## 📋 How It Works

### Process Flow

1. **Upload Excel File**
   - User selects an Excel file containing payment records
   - File is read using EPPlus library
   - Data is displayed in a data grid

2. **Validate Payments**
   - Click the "Validate Payment" button
   - For each row in the Excel file
     - Download payment slip image from Google Drive link
     - Preprocess the image for optimal OCR results
     - Extract text data using Tesseract OCR
     - Compare extracted data with Excel column values
     - Mark as **Valid** if data matches, **Invalid** otherwise

3. **View Results**
   - Summary shows total valid and invalid payment counts
   - Records displayed with color-coded validation status:
     - 🟢 **Green**: Valid payments
     - 🔴 **Red**: Invalid payments
   - New validation column added to clearly separate results

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or later
- Tesseract language data files (`tessdata`)

### Installation

1. Clone the repository: https://github.com/hansanakavinda/WinForm_Payment_Validator.git