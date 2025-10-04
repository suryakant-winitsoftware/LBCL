#!/usr/bin/env python3
"""
Auto-run script that periodically checks for changes in the GitHub repository
and automatically pulls and runs the .NET application when changes are detected.
"""

import os
import sys
import time
import subprocess
import requests
import json
import hashlib
import signal
import atexit
from datetime import datetime
from pathlib import Path

# Configuration
REPO_URL = "https://github.com/amitwinit/Multiplex.git"
REPO_NAME = "Multiplex"
BRANCH = "main"
CHECK_INTERVAL = 60  # Check every 60 seconds
API_BASE_URL = "https://api.github.com"
REPO_OWNER = "amitwinit"
REPO_NAME_API = "Multiplex"

class AutoRunner:
    def __init__(self):
        self.current_commit_hash = None
        self.process = None
        self.is_running = False
        self.original_dir = os.getcwd()
        self.script_path = os.path.abspath(__file__)
        self.restart_needed = False
        
        # Register cleanup function
        atexit.register(self.cleanup)
        
    def cleanup(self):
        """Cleanup function called on exit"""
        self.stop_dotnet_app()
        
    def log(self, message):
        """Log message with timestamp"""
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        print(f"[{timestamp}] {message}")
        
    def get_latest_commit_hash(self, repo_root):
        """Get the latest commit hash using local git commands"""
        try:
            original_dir = os.getcwd()
            os.chdir(repo_root)
            
            # Fetch latest changes from remote
            subprocess.run(["git", "fetch", "origin"], check=True, capture_output=True)
            
            # Get the latest commit hash from the remote branch
            result = subprocess.run(
                ["git", "rev-parse", f"origin/{BRANCH}"], 
                capture_output=True, text=True, check=True
            )
            commit_hash = result.stdout.strip()
            
            os.chdir(original_dir)
            return commit_hash
            
        except subprocess.CalledProcessError as e:
            self.log(f"Error getting latest commit hash: {e}")
            return None
        except Exception as e:
            self.log(f"Error getting latest commit hash: {e}")
            return None
            
    def get_current_commit_hash(self, repo_root):
        """Get the current local commit hash"""
        try:
            original_dir = os.getcwd()
            os.chdir(repo_root)
            
            result = subprocess.run(
                ["git", "rev-parse", "HEAD"], 
                capture_output=True, text=True, check=True
            )
            commit_hash = result.stdout.strip()
            
            os.chdir(original_dir)
            return commit_hash
            
        except subprocess.CalledProcessError as e:
            self.log(f"Error getting current commit hash: {e}")
            return None
        except Exception as e:
            self.log(f"Error getting current commit hash: {e}")
            return None
            
    def find_repository_root(self):
        """Find the repository root directory by looking for .git folder"""
        current_dir = os.getcwd()
        
        # Check if we're already in a git repository
        if os.path.exists(os.path.join(current_dir, '.git')):
            return current_dir
            
        # Look for .git folder in parent directories
        parent_dir = os.path.dirname(current_dir)
        while parent_dir != current_dir:
            if os.path.exists(os.path.join(parent_dir, '.git')):
                return parent_dir
            current_dir = parent_dir
            parent_dir = os.path.dirname(current_dir)
            
        return None
        
    def clone_repository(self):
        """Clone the repository if it doesn't exist"""
        repo_root = self.find_repository_root()
        
        if repo_root:
            self.log(f"Found existing repository at: {repo_root}")
            return repo_root
            
        # If no repository found, clone it
        if not os.path.exists(REPO_NAME):
            self.log(f"Cloning repository {REPO_URL}...")
            try:
                result = subprocess.run(
                    ["git", "clone", REPO_URL],
                    capture_output=True,
                    text=True,
                    check=True
                )
                self.log("Repository cloned successfully")
                return os.path.abspath(REPO_NAME)
            except subprocess.CalledProcessError as e:
                self.log(f"Error cloning repository: {e}")
                return None
        else:
            return os.path.abspath(REPO_NAME)
        
    def pull_changes(self, repo_root):
        """Pull latest changes from the repository"""
        try:
            original_dir = os.getcwd()
            os.chdir(repo_root)
            self.log("Pulling latest changes...")
            
            # Fetch latest changes
            subprocess.run(["git", "fetch", "origin"], check=True, capture_output=True)
            
            # Check if we're on the main branch
            result = subprocess.run(["git", "branch", "--show-current"], 
                                  capture_output=True, text=True, check=True)
            current_branch = result.stdout.strip()
            
            if current_branch != BRANCH:
                self.log(f"Switching to {BRANCH} branch...")
                subprocess.run(["git", "checkout", BRANCH], check=True, capture_output=True)
            
            # Try to pull changes
            try:
                # Check if our script file will be modified
                script_relative_path = os.path.relpath(self.script_path, repo_root)
                if os.path.exists(script_relative_path):
                    # Get the current hash of our script
                    with open(self.script_path, 'rb') as f:
                        current_script_hash = hashlib.md5(f.read()).hexdigest()
                
                # Pull changes
                result = subprocess.run(["git", "pull", "origin", BRANCH], 
                                      capture_output=True, text=True, check=True)
                
                # Check if our script was modified
                if os.path.exists(script_relative_path):
                    with open(self.script_path, 'rb') as f:
                        new_script_hash = hashlib.md5(f.read()).hexdigest()
                    
                    if current_script_hash != new_script_hash:
                        self.log("Script file was updated! Restarting script...")
                        self.restart_needed = True
                        return True
                        
            except subprocess.CalledProcessError as e:
                # Handle specific git errors
                error_output = e.stderr if e.stderr else e.stdout
                if "untracked working tree files would be overwritten" in error_output:
                    self.log("Untracked files would be overwritten. Adding data directories to .gitignore...")
                    
                    # Add common data directories to .gitignore
                    gitignore_path = os.path.join(repo_root, ".gitignore")
                    data_patterns = [
                        "# Auto-generated data directories",
                        "WINITAPI/Data/Sqlite/",
                        "WINITAPI/Data/SqliteLog/",
                        "WINITAPI/Data/SyncLog/",
                        "python-tools/",
                        "*.db",
                        "*.log"
                    ]
                    
                    # Read existing .gitignore
                    existing_content = ""
                    if os.path.exists(gitignore_path):
                        with open(gitignore_path, 'r') as f:
                            existing_content = f.read()
                    
                    # Add new patterns if they don't exist
                    new_patterns = []
                    for pattern in data_patterns:
                        if pattern not in existing_content:
                            new_patterns.append(pattern)
                    
                    if new_patterns:
                        with open(gitignore_path, 'a') as f:
                            f.write("\n" + "\n".join(new_patterns) + "\n")
                        self.log("Updated .gitignore with data directory patterns")
                    
                    # Try to clean untracked files that would be overwritten
                    try:
                        self.log("Cleaning untracked files that would be overwritten...")
                        subprocess.run(["git", "clean", "-fd"], check=True, capture_output=True)
                        self.log("Cleaned untracked files successfully")
                        
                        # Try pull again
                        result = subprocess.run(["git", "pull", "origin", BRANCH], 
                                              capture_output=True, text=True, check=True)
                        self.log("Pull successful after cleaning untracked files")
                        
                    except subprocess.CalledProcessError as clean_error:
                        self.log(f"Failed to clean untracked files: {clean_error}")
                        self.log("Manual intervention required. Please run 'git clean -fd' and 'git pull' manually.")
                        return False
                        
                else:
                    # Re-raise other git errors
                    raise e
            
            self.log("Changes pulled successfully")
            return True
            
        except subprocess.CalledProcessError as e:
            self.log(f"Error pulling changes: {e}")
            if e.stderr:
                self.log(f"Git error details: {e.stderr}")
            return False
        except Exception as e:
            self.log(f"Unexpected error during pull: {e}")
            return False
        finally:
            # Go back to the original directory
            os.chdir(original_dir)
            
    def restart_script(self):
        """Restart the script with the updated version"""
        self.log("Restarting script with updated version...")
        self.stop_dotnet_app()
        
        # Start the new version of the script
        try:
            subprocess.Popen([sys.executable, self.script_path])
            self.log("New script instance started successfully")
            sys.exit(0)
        except Exception as e:
            self.log(f"Error restarting script: {e}")
            
    def stop_dotnet_app(self):
        """Stop the running .NET application"""
        if self.process and self.is_running:
            self.log("Stopping .NET application...")
            try:
                self.process.terminate()
                self.process.wait(timeout=10)
                self.log("DotNET application stopped")
            except subprocess.TimeoutExpired:
                self.log("Force killing .NET application...")
                self.process.kill()
                self.process.wait()
            except Exception as e:
                self.log(f"Error stopping .NET application: {e}")
            finally:
                self.process = None
                self.is_running = False
                
    def start_dotnet_app(self, repo_root):
        """Start the .NET application"""
        try:
            # Look for WINITAPI directory relative to repository root
            winit_api_path = os.path.join(repo_root, "WINITAPI")
            
            # If not found in root, check if we're already in WINITAPI or backend
            if not os.path.exists(winit_api_path):
                # Check if we're in a backend directory that contains WINITAPI
                current_dir = os.getcwd()
                if os.path.basename(current_dir) == "backend":
                    winit_api_path = os.path.join(current_dir, "WINITAPI")
                elif os.path.basename(current_dir) == "WINITAPI":
                    winit_api_path = current_dir
                else:
                    # Look for WINITAPI in current directory or subdirectories
                    for root, dirs, files in os.walk(current_dir):
                        if "WINITAPI" in dirs:
                            winit_api_path = os.path.join(root, "WINITAPI")
                            break
            
            if not os.path.exists(winit_api_path):
                self.log(f"WINITAPI directory not found. Searched in: {winit_api_path}")
                self.log("Current directory structure:")
                self.log_directory_structure(repo_root)
                return False
                
            self.log(f"Starting .NET application from: {winit_api_path}")
            original_dir = os.getcwd()
            os.chdir(winit_api_path)
            
            # Start the dotnet application without capturing output so logs are visible
            self.process = subprocess.Popen(
                ["dotnet", "run"],
                # Remove stdout and stderr capture to see .NET logs
                # stdout=subprocess.PIPE,
                # stderr=subprocess.PIPE,
                # text=True
            )
            
            self.is_running = True
            self.log("DotNET application started successfully")
            return True
            
        except Exception as e:
            self.log(f"Error starting .NET application: {e}")
            return False
        finally:
            # Go back to the original directory
            os.chdir(self.original_dir)
            
    def log_directory_structure(self, start_path, max_depth=3, current_depth=0):
        """Log the directory structure to help debug WINITAPI location"""
        if current_depth >= max_depth:
            return
            
        try:
            for item in os.listdir(start_path):
                item_path = os.path.join(start_path, item)
                if os.path.isdir(item_path):
                    self.log(f"{'  ' * current_depth}📁 {item}")
                    if current_depth < max_depth - 1:
                        self.log_directory_structure(item_path, max_depth, current_depth + 1)
        except PermissionError:
            self.log(f"{'  ' * current_depth}❌ Permission denied: {start_path}")
            
    def check_and_update(self, repo_root):
        """Check for changes and update if necessary"""
        # Get the latest commit hash from remote
        latest_hash = self.get_latest_commit_hash(repo_root)
        if not latest_hash:
            return
            
        # Get current local commit hash
        current_hash = self.get_current_commit_hash(repo_root)
        if not current_hash:
            return
            
        # If this is the first run or hash has changed
        if self.current_commit_hash != latest_hash:
            self.log(f"Change detected! Remote commit: {latest_hash[:8]}, Local commit: {current_hash[:8]}")
            
            # Stop the current application
            self.stop_dotnet_app()
            
            # Pull the latest changes
            if self.pull_changes(repo_root):
                # Check if we need to restart the script
                if self.restart_needed:
                    self.restart_script()
                    return
                
                # Start the application with new changes
                if self.start_dotnet_app(repo_root):
                    self.current_commit_hash = latest_hash
                    self.log("Application updated and restarted successfully")
                else:
                    self.log("Failed to start application after update")
            else:
                self.log("Failed to pull changes")
        else:
            self.log("No changes detected")
            
    def run(self):
        """Main loop to continuously monitor for changes"""
        self.log("Starting auto-run script...")
        self.log(f"Monitoring repository: {REPO_URL}")
        self.log(f"Branch: {BRANCH}")
        self.log(f"Check interval: {CHECK_INTERVAL} seconds")
        self.log(f"Current working directory: {os.getcwd()}")
        self.log(f"Script path: {self.script_path}")
        
        # Find or clone repository
        repo_root = self.clone_repository()
        if not repo_root:
            self.log("Failed to find or clone repository. Exiting...")
            return
            
        self.log(f"Using repository at: {repo_root}")
        
        # Initial setup
        self.check_and_update(repo_root)
        
        try:
            while True:
                time.sleep(CHECK_INTERVAL)
                self.check_and_update(repo_root)
                
        except KeyboardInterrupt:
            self.log("Received interrupt signal. Shutting down...")
            self.stop_dotnet_app()
            self.log("Auto-run script stopped")

def main():
    """Main entry point"""
    # Check if required tools are available
    try:
        subprocess.run(["git", "--version"], capture_output=True, check=True)
        subprocess.run(["dotnet", "--version"], capture_output=True, check=True)
    except (subprocess.CalledProcessError, FileNotFoundError):
        print("Error: git and dotnet must be installed and available in PATH")
        sys.exit(1)
        
    # Create and run the auto-runner
    auto_runner = AutoRunner()
    auto_runner.run()

if __name__ == "__main__":
    main()
