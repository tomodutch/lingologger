# app.py
from flask import Flask, request, send_file
import matplotlib
import pandas as pd
import seaborn as sns
import bar_chart_race as bcr
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
import os
import io

matplotlib.use('Agg')  # Use Agg backend for non-GUI environments
sns.set_theme(style="whitegrid", font_scale=1.2)
plt.rcParams.update({
    'axes.facecolor': '#36393F',  # Dark gray for the axes background
    'figure.facecolor': '#36393F',  # Dark gray for the entire figure background
    'axes.edgecolor': '#2F3136',  # Slightly lighter gray for the edges
    'axes.labelcolor': 'lightgray',  # Light gray labels for contrast
    'xtick.color': '#B9BBBE',  # Light gray tick color
    'ytick.color': '#B9BBBE',  # Light gray tick color
    'grid.color': '#4F545C',  # Dark gridlines to mimic subtle Discord accents
    'text.color': 'lightgray',  # Light gray for any general text
    'axes.titleweight': 'bold',  # Bolder title for emphasis,
    'hatch.color':     'blue',
    'hatch.linewidth': 0.3
})

app = Flask(__name__)

@app.route('/generate_barchart', methods=['POST'])
def generate_barchart():
    # Parse JSON data from request
    data = request.json.get("data")
    title = request.json.get("title", "Bar Chart")
    index = request.json.get("index")
    x_axis_title = request.json.get("xAxisTitle")
    y_axis_title = request.json.get("yAxisTitle")
    columns = list(data.keys())

    # Split columns into two groups: stacked and individual
    stacked_columns = columns  # First two as stacked

    # Define colors and hatches
    colors = ["#E69F00", "#56B4E9", "#009E73", "#F0E442"]
    hatch_patterns = ['x', '+', '|', 'O']

    # Create DataFrame for both stacked and individual bars
    df_stacked = pd.DataFrame({col: data[col] for col in stacked_columns}, index=index)

    # Plot stacked bars
    ax = df_stacked.plot(kind="bar", stacked=True, figsize=(10, 6), color=colors[:len(stacked_columns)], edgecolor=None, position=0, width=0.4)
    # Set legend with custom handles
    legend_handles = [
        mpatches.Patch(facecolor=colors[i], hatch=hatch_patterns[i % len(hatch_patterns)], edgecolor="white", label=columns[i], linewidth=1)
        for i in range(len(columns))
    ]
    ax.legend(handles=legend_handles, title="Categories", frameon=False, labelcolor='lightgray', loc='upper left', bbox_to_anchor=(1, 1))

    # Set title and labels
    ax.set_title(title, fontsize=16, fontweight='bold', color='lightgray')
    ax.set_xlabel(x_axis_title, fontsize=12, fontweight='bold', color='lightgray')
    ax.set_ylabel(y_axis_title, fontsize=12, fontweight='bold', color='lightgray')

    # Customize tick labels
    ax.tick_params(axis="x", rotation=45, labelsize=10, labelcolor='#B9BBBE')
    ax.tick_params(axis="y", labelsize=10, labelcolor='#B9BBBE')

    # Set hatches on stacked bars only
    for i, bar in enumerate(ax.containers[:len(stacked_columns)]):
        for patch in bar:
            patch.set_hatch(hatch_patterns[i % len(hatch_patterns)])

    # Adjust layout and save to buffer
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
