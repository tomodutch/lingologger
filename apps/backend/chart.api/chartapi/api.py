# app.py
from flask import Flask, request, send_file
import matplotlib
import pandas as pd
import bar_chart_race as bcr
import matplotlib.pyplot as plt
import os
import io

matplotlib.use('Agg')  # Use Agg backend for non-GUI environments
app = Flask(__name__)

@app.route('/generate_barchart', methods=['POST'])
def generate_barchart():
    # Parse JSON data from request
    data = request.json.get("data")
    title = request.json.get("title", "Bar Chart")
    index = request.json.get("index")

    # Create a DataFrame
    df = pd.DataFrame(data, index)
    # Plot the bar chart
    plt.figure(figsize=(10, 6))
    df.plot(kind="bar")
    plt.title(title)
    plt.xlabel("Categories")
    plt.ylabel("Values")
    plt.tight_layout()
    buffer = io.BytesIO()
    plt.savefig(buffer, format='png')
    buffer.seek(0)
    plt.close()

    # Send the generated bar chart image as response
    response = send_file(buffer, mimetype='image/png', as_attachment=False, download_name="barchart.png")

    return response

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
