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

    # Define a color-blind-friendly palette with increased contrast
    colors = ["#E69F00", "#56B4E9", "#009E73", "#F0E442"]
    # *+-./OX\ox|
    hatch_patterns = ['x', '+', '|', 'O']  # Different hatch patterns for each category

    # Create a DataFrame
    df = pd.DataFrame(data, index)
    # Plot with a Seaborn barplot
    # ax = sns.barplot(data=df, ci=None, edgecolor=None)
    ax = df.plot(kind="bar", stacked=True, figsize=(10, 6), color=colors, edgecolor=None, alpha=1.0)

    # Create custom legend handles with hatches
    legend_handles = [
        mpatches.Patch(
            facecolor=colors[i], hatch=hatch_patterns[i % len(hatch_patterns)],
            edgecolor="white", label=columns[i], linewidth=1
        ) for i in range(len(columns))
    ]
    ax.legend(handles=legend_handles, title="Categories", frameon=False, labelcolor='lightgray', loc='upper left', bbox_to_anchor=(1, 1))

    # Set title and labels
    ax.set_title(title, fontsize=16, fontweight='bold', color='lightgray')
    ax.set_xlabel(x_axis_title, fontsize=12, fontweight='bold', color='lightgray')
    ax.set_ylabel(y_axis_title, fontsize=12, fontweight='bold', color='lightgray')
    
    # Customize the tick labels
    ax.tick_params(axis="x", rotation=45, labelsize=10, labelcolor='#B9BBBE')
    ax.tick_params(axis="y", labelsize=10, labelcolor='#B9BBBE')
    # Remove bar borders (set linewidth=0)
    for p in ax.patches:
        p.set_edgecolor('white')
        p.set_linewidth(0)  # Remove the border's width
        # p.set_hatch("x")

    for i, bar in enumerate(ax.containers):
        for patch in bar:
            patch.set_hatch(hatch_patterns[i % len(hatch_patterns)])
    # Add value labels above bars for extra clarity
    # for p in ax.patches:
    #     ax.annotate(f'{p.get_height():,.0f}', 
    #                 (p.get_x() + p.get_width() / 2., p.get_height()), 
    #                 ha='center', va='center', 
    #                 fontsize=11, color='white', 
    #                 xytext=(0, 9), textcoords='offset points')
        
    # Plot the bar chart
    # plt.figure(figsize=(10, 6))
    # df.plot(kind="bar")
    # plt.title(title)
    # plt.xlabel("Categories")
    # plt.ylabel("Values")
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
