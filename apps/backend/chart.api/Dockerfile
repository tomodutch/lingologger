# Use the official Python 3 image as a base
FROM python:3.10-slim

# Set the working directory in the container
WORKDIR /app

# Copy the requirements file to the container
COPY /apps/backend/chart.api/requirements.txt /app/

# Install the Python dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Copy the application code to the container
COPY /apps/backend/chart.api /app/

# Expose the port the app will run on
EXPOSE 5000

# Run the Flask application using Gunicorn
CMD ["gunicorn", "--bind", "0.0.0.0:5000", "chartapi.api:app"]
