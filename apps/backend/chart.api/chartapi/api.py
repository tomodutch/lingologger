from flask import Flask, request, send_file
import plotly.graph_objects as go
from io import BytesIO

app = Flask(__name__)

@app.route('/generate_barchart', methods=['POST'])
def generate_barchart():
    # Parse JSON data from request
    data = request.json.get("data")
    title = request.json.get("title", "Bar Chart")
    index = request.json.get("index")
    x_axis_title = request.json.get("xAxisTitle", "X-Axis")
    y_axis_title = request.json.get("yAxisTitle", "Y-Axis")
    columns = list(data.keys())

    # Prepare data for the stacked bar chart
    fig = go.Figure()

    # Define colors and hatches
    colors = ["#E69F00", "#56B4E9", "#009E73", "#F0E442"]

    # Add each column as a separate trace
    for i, column in enumerate(columns):
        fig.add_trace(go.Bar(
            name=column,
            x=index,
            y=data[column],
            marker=dict(
                color=colors[i % len(colors)],
                pattern=dict(shape=['x', '+', '|', '/'][i % 4])
            ),
            opacity=0.8  # Adjust opacity for all traces
        ))

    # Update layout for the stacked bar chart
    fig.update_layout(
        barmode='stack',
        title=dict(text=title, font=dict(size=16, color='lightgray'), x=0.5),
        xaxis=dict(
            title=dict(text=x_axis_title, font=dict(size=12, color='lightgray')),
            tickfont=dict(size=10, color='#B9BBBE'),
            gridcolor='#4F545C',  # Dim the gridlines
            gridwidth=0.5  # Make the gridlines thinner
        ),
        yaxis=dict(
            title=dict(text=y_axis_title, font=dict(size=12, color='lightgray')),
            tickfont=dict(size=10, color='#B9BBBE'),
            gridcolor='#4F545C',  # Dim the gridlines
            gridwidth=0.5  # Make the gridlines thinner
        ),
        legend=dict(
            title=dict(text="Categories", font=dict(size=12, color='lightgray')),
            font=dict(color='lightgray'),
            bgcolor='rgba(54, 57, 63, 0.8)',
            x=1, y=1
        ),
        plot_bgcolor='#36393F',
        paper_bgcolor='#36393F',
        margin=dict(t=50, b=40, l=40, r=150)  # Adjust layout to fit legend
    )

    # Write to a buffer
    buffer = BytesIO()
    fig.write_image(buffer, format='png')
    buffer.seek(0)

    # Send the generated bar chart image as response
    response = send_file(buffer, mimetype='image/png', as_attachment=False, download_name="barchart.png")

    return response

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
