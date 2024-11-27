from flask import Flask, request, send_file
import plotly.graph_objects as go
import plotly.express as px
from plotly.subplots import make_subplots
from io import BytesIO
import pandas as pd

app = Flask(__name__)

def create_barchart(data, index, title="Bar Chart", x_axis_title="X-Axis", y_axis_title="Y-Axis", theme="dark"):
    """
    Create a bar chart using the provided data and theme.
    """
    # Define colors based on theme
    if theme == "white":
        colors = ["#377EB8", "#4DAF4A", "#FF7F00", "#984EA3"]
        bg_color = "#FFFFFF"
        font_color = "#000000"
        grid_color = "#DDDDDD"
        legend_bg = "rgba(255, 255, 255, 0.8)"
    else:
        colors = ["#E69F00", "#56B4E9", "#009E73", "#F0E442"]
        bg_color = "#36393F"
        font_color = "lightgray"
        grid_color = "#4F545C"
        legend_bg = "rgba(54, 57, 63, 0.8)"

    # Prepare the figure
    fig = go.Figure()

    # Add each column as a separate trace
    for i, column in enumerate(data.keys()):
        fig.add_trace(go.Bar(
            name=column,
            x=index,
            y=data[column],
            marker=dict(
                color=colors[i % len(colors)],
                pattern=dict(shape=['x', '+', '|', '/'][i % 4])
            ),
            opacity=0.8
        ))

    # Update layout
    fig.update_layout(
        barmode='stack',
        title=dict(text=title, font=dict(size=16, color=font_color), x=0.5),
        xaxis=dict(
            title=dict(text=x_axis_title, font=dict(size=12, color=font_color)),
            tickfont=dict(size=10, color=font_color),
            gridcolor=grid_color,
            gridwidth=0.5
        ),
        yaxis=dict(
            title=dict(text=y_axis_title, font=dict(size=12, color=font_color)),
            tickfont=dict(size=10, color=font_color),
            gridcolor=grid_color,
            gridwidth=0.5
        ),
        legend=dict(
            title=dict(text="Categories", font=dict(size=12, color=font_color)),
            font=dict(color=font_color),
            bgcolor=legend_bg,
            x=1, y=1
        ),
        plot_bgcolor=bg_color,
        paper_bgcolor=bg_color,
        margin=dict(t=50, b=40, l=40, r=150)
    )

    return fig

@app.route('/generate_dashboard', methods=['POST'])
def generate_dashboard():
    try:
        # Parse JSON request
        data = request.get_json()
        if not data:
            return {"error": "No data provided in the request body"}, 400

        # Extract theme and data
        theme = data.get("theme", "dark").lower()
        bar = data.get("bar", {})
        burndowns = data.get("burndowns", [])

        # Define theme-based styles
        if theme == "white":
            bg_color = "#FFFFFF"
            font_color = "#000000"
        else:
            bg_color = "#36393F"
            font_color = "lightgray"
        rows = 2 + len(burndowns)
        specs = [[{"colspan": 2}, None]] * rows
        # Create subplots layout
        fig = make_subplots(
            rows=rows, cols=2,
            specs=specs,
            # subplot_titles=("Bar Chart", "Burndown Chart", f"Avg Hours/Day: {avg_hours:.2f}"),
            vertical_spacing=0.05
        )

        # Add Bar Chart using generate_barchart
        if bar:
            # fig = create_barchart(data, index, title, x_axis_title, y_axis_title, theme)
            barchart_fig = create_barchart(bar["data"], bar["index"], "", theme=theme)
            for trace in barchart_fig.data:
                fig.add_trace(trace, row=1, col=1)

        for i, burndown in enumerate(burndowns):
            # Add Burndown Chart
            df = pd.DataFrame(burndown)
            
            # Convert the 'date' column to datetime format, this will handle both date and time
            df['date'] = pd.to_datetime(df['date'], errors='coerce')
            
            # Sort by date to ensure it's in chronological order
            df = df.sort_values('date')
            
            row = i + 2
            # Calculate the expected work line
            start_work = df['remaining_work'].iloc[0]
            end_work = 0
            
            # For both date and time, calculate the total days or seconds for interpolation
            total_days = (df['date'].iloc[-1] - df['date'].iloc[0]).days
            df['expected_work'] = start_work - (start_work - end_work) * (df['date'] - df['date'].iloc[0]).dt.days / total_days
            
            # Add Actual Work trace
            fig.add_trace(go.Scatter(
                x=df['date'], y=df['remaining_work'],
                mode='lines+markers',
                name='Actual Work',
                line=dict(color="blue", width=2)
            ), row=row, col=1)

            # Add Expected Work trace
            fig.add_trace(go.Scatter(
                x=df['date'], y=df['expected_work'],
                mode='lines',
                name='Expected Speed',
                line=dict(color="orange", dash="dot", width=2)
            ), row=row, col=1)

        # Update layout with theme styles
        fig.update_layout(
            height=len(specs) * 400,
            barmode='stack',
            paper_bgcolor=bg_color,
            font=dict(color=font_color),
            title=dict(text="Dashboard", x=0.5, font=dict(size=20)),
            showlegend=False)

        # Export the dashboard to a PNG
        buffer = BytesIO()
        fig.write_image(buffer, format='png', scale=2)
        buffer.seek(0)

        # Return the image
        return send_file(buffer, mimetype='image/png', as_attachment=False, download_name="dashboard.png")

    except Exception as e:
        return {"error": str(e)}, 500

@app.route('/generate_barchart', methods=['POST'])
def generate_barchart_endpoint():
    try:
        # Parse JSON data from request
        data = request.json.get("data")
        index = request.json.get("index")
        title = request.json.get("title", "Bar Chart")
        x_axis_title = request.json.get("xAxisTitle", "X-Axis")
        y_axis_title = request.json.get("yAxisTitle", "Y-Axis")
        theme = request.json.get("theme", "dark").lower()

        # Generate the bar chart
        fig = create_barchart(data, index, title, x_axis_title, y_axis_title, theme)

        # Write to a buffer
        buffer = BytesIO()
        fig.write_image(buffer, format='png', scale=2)
        buffer.seek(0)

        # Send the generated bar chart image as response
        return send_file(buffer, mimetype='image/png', as_attachment=False, download_name="barchart.png")

    except Exception as e:
        return {"error": str(e)}, 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
