#!/bin/bash

echo "========================================="
echo "Starting Backend Services"
echo "========================================="

# Check if RabbitMQ is installed
if command -v rabbitmq-server &> /dev/null; then
    echo "✓ RabbitMQ is installed"
    
    # Check if RabbitMQ is running
    if rabbitmqctl status &> /dev/null; then
        echo "✓ RabbitMQ is already running"
    else
        echo "Starting RabbitMQ..."
        # Try to start RabbitMQ in the background
        rabbitmq-server -detached
        
        # Wait a few seconds for RabbitMQ to start
        sleep 5
        
        # Check if it started successfully
        if rabbitmqctl status &> /dev/null; then
            echo "✓ RabbitMQ started successfully"
        else
            echo "⚠ Failed to start RabbitMQ. Trying with brew services..."
            # If direct start failed, try with brew services (macOS)
            if command -v brew &> /dev/null; then
                brew services start rabbitmq
                sleep 5
                if rabbitmqctl status &> /dev/null; then
                    echo "✓ RabbitMQ started with brew services"
                else
                    echo "✗ Failed to start RabbitMQ"
                fi
            fi
        fi
    fi
    
    # Display RabbitMQ connection info
    echo ""
    echo "RabbitMQ Connection Info:"
    echo "  Host: localhost"
    echo "  Port: 5672"
    echo "  Management UI: http://localhost:15672"
    echo "  Username: guest"
    echo "  Password: guest"
    echo ""
else
    echo "⚠ RabbitMQ is not installed"
    echo ""
    echo "To install RabbitMQ on macOS:"
    echo "  brew install rabbitmq"
    echo ""
    echo "To install RabbitMQ on Ubuntu/Debian:"
    echo "  sudo apt-get install rabbitmq-server"
    echo ""
fi

# Check if PostgreSQL is running (optional)
if command -v pg_ctl &> /dev/null || command -v psql &> /dev/null; then
    if pg_isready &> /dev/null; then
        echo "✓ PostgreSQL is running"
    else
        echo "⚠ PostgreSQL is not running"
        echo "  You may need to start PostgreSQL manually"
    fi
else
    echo "⚠ PostgreSQL not found"
fi

# Check if Redis is running (optional)
if command -v redis-cli &> /dev/null; then
    if redis-cli ping &> /dev/null; then
        echo "✓ Redis is running"
    else
        echo "⚠ Redis is not running"
        echo "  Starting Redis might improve performance"
    fi
else
    echo "⚠ Redis not found"
fi

echo ""
echo "========================================="
echo "Starting .NET Backend Server"
echo "========================================="
echo ""

# Navigate to the API directory
cd WINITAPI

# Run the .NET application
echo "Running: dotnet run"
dotnet run

# If dotnet run fails, try dotnet watch
if [ $? -ne 0 ]; then
    echo ""
    echo "dotnet run failed, trying dotnet watch run..."
    dotnet watch run
fi