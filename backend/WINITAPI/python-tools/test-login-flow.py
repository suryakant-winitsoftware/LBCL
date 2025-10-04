#!/usr/bin/env python3
"""
WINIT Mobile App Login Flow Test Script
=======================================

This script replicates the exact login flow from the React Native app:
1. Authenticate user with encrypted credentials
2. Initiate database creation on server
3. Poll for database creation status
4. Download database file (simulated)
5. Log all steps in detail

Based on the React Native app's authentication and sync flow.
"""

import requests
import json
import time
import hashlib
import base64
import logging
from typing import Dict, Any, Optional
from urllib.parse import urljoin, urlencode
import os
from datetime import datetime

# Configure detailed logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('login_flow_test.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

class WINITLoginFlowTester:
    """Replicates the exact WINIT Mobile app login flow"""
    
    def __init__(self, base_url: str):
        self.base_url = base_url.rstrip('/')
        self.session = requests.Session()
        self.auth_token = None
        self.user_data = None
        
        # Configure session headers
        self.session.headers.update({
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'User-Agent': 'WINITMobileApp/1.0.0'
        })
        
        logger.info("Initialized WINIT Login Flow Tester")
        logger.info(f"Base URL: {self.base_url}")
    
    def log_request(self, method: str, url: str, data: Any = None, headers: Dict = None):
        """Log detailed request information"""
        logger.info(f"OUTGOING {method} Request:")
        logger.info(f"   URL: {url}")
        if headers:
            logger.info(f"   Headers: {json.dumps(headers, indent=2)}")
        if data:
            logger.info(f"   Data: {json.dumps(data, indent=2)}")
        logger.info("-" * 80)
    
    def log_response(self, response: requests.Response):
        """Log detailed response information"""
        logger.info(f"INCOMING Response:")
        logger.info(f"   Status: {response.status_code}")
        logger.info(f"   Headers: {dict(response.headers)}")
        try:
            response_data = response.json()
            logger.info(f"   Data: {json.dumps(response_data, indent=2)}")
        except:
            logger.info(f"   Text: {response.text[:500]}...")
        logger.info("-" * 80)
    
    def generate_challenge_code(self) -> str:
        """
        Generate challenge code in the format: yyyyMMddHHmmss
        Equivalent to C# DateTime.UtcNow.ToString("yyyyMMddHHmmss")
        """
        now = datetime.utcnow()
        return now.strftime("%Y%m%d%H%M%S")
    
    def hash_password_with_sha256(self, input_str: str) -> str:
        """
        Hash input using SHA256 and return as Base64 string
        Equivalent to C# SHA256.ComputeHash + Convert.ToBase64String
        """
        hash_obj = hashlib.sha256(input_str.encode('utf-8'))
        return base64.b64encode(hash_obj.digest()).decode('utf-8')
    
    def encrypt_password_with_challenge(self, password: str, challenge: str) -> str:
        """
        Encrypt password with challenge code
        Equivalent to C# EncryptPasswordWithChallenge method
        """
        password_with_challenge = password + challenge
        return self.hash_password_with_sha256(password_with_challenge)
    
    def prepare_auth_data(self, user_id: str, password: str) -> Dict[str, Any]:
        """
        Replicate the exact authentication data preparation from the React Native app
        Based on src/utils/encryption.ts prepareAuthData function
        """
        logger.info("Preparing authentication data...")
        
        # Generate challenge code in correct format
        challenge_code = self.generate_challenge_code()
        
        # Encrypt password with challenge
        encrypted_password = self.encrypt_password_with_challenge(password, challenge_code)
        
        # Prepare device info (matching React Native app)
        device_info = {
            "platform": "React Native",
            "version": "1.0.0",
            "deviceId": "RN_DEVICE_001"
        }
        
        auth_data = {
            "userID": user_id,
            "password": encrypted_password,
            "challengeCode": challenge_code,
            "deviceInfo": device_info
        }
        
        logger.info("Authentication data prepared:")
        logger.info(f"   User ID: {user_id}")
        logger.info(f"   Challenge Code: {challenge_code}")
        logger.info(f"   Device Info: {json.dumps(device_info, indent=2)}")
        
        return auth_data
    
    def authenticate_user(self, user_id: str, password: str) -> bool:
        """
        Step 1: Authenticate user with the server
        Based on AuthApiService.login method
        """
        logger.info("STEP 1: Starting user authentication...")
        
        # Prepare authentication data
        auth_data = self.prepare_auth_data(user_id, password)
        
        # Make authentication request
        auth_url = f"{self.base_url}/api/Auth/GetToken"
        self.log_request("POST", auth_url, auth_data)
        
        try:
            response = self.session.post(auth_url, json=auth_data, timeout=30)
            self.log_response(response)
            
            if response.status_code == 200:
                response_data = response.json()
                
                if response_data.get('IsSuccess', False):
                    # Extract token and user data
                    self.auth_token = response_data.get('Data', {}).get('Token')
                    self.user_data = response_data.get('Data', {}).get('AuthMaster')
                    
                    if self.auth_token and self.user_data:
                        logger.info("SUCCESS: Authentication successful!")
                        logger.info(f"   Token: {self.auth_token[:20]}...")
                        logger.info(f"   User Data: {json.dumps(self.user_data, indent=2)}")
                        
                        # Update session headers with auth token
                        self.session.headers.update({
                            'Authorization': f'Bearer {self.auth_token}'
                        })
                        
                        return True
                    else:
                        logger.error("ERROR: Missing token or user data in response")
                        return False
                else:
                    logger.error(f"ERROR: Authentication failed - {response_data.get('ErrorMessage', 'Unknown error')}")
                    return False
            else:
                logger.error(f"ERROR: Authentication failed with status {response.status_code}")
                return False
                
        except requests.exceptions.RequestException as e:
            logger.error(f"ERROR: Request failed - {str(e)}")
            return False
    
    def initiate_database_creation(self) -> bool:
        """
        Step 2: Initiate database creation on server
        Based on WINITDatabaseDownloader.initiateDBCreation method
        """
        logger.info("STEP 2: Initiating database creation...")
        
        if not self.auth_token or not self.user_data:
            logger.error("ERROR: Authentication required before initiating database creation")
            return False
        
        # Prepare user context for database creation
        user_context = {
            "empUID": self.user_data.get("Emp", {}).get("UID"),
            "empCode": self.user_data.get("Emp", {}).get("Code"),
            "jobPositionUID": self.user_data.get("JobPosition", {}).get("UID"),
            "roleUID": self.user_data.get("Role", {}).get("UID"),
            "vehicleUID": None,  # Not provided in response
            "orgUID": self.user_data.get("JobPosition", {}).get("OrgUID"),
            "authToken": self.auth_token
        }
        
        logger.info(f"User Context: {json.dumps(user_context, indent=2)}")
        
        # Make database creation request using query parameters (like React Native app)
        params = {
            'employeeUID': user_context['empUID'],
            'jobPositionUID': user_context['jobPositionUID'],
            'roleUID': user_context['roleUID'],
            'orgUID': user_context['orgUID'],
            'vehicleUID': user_context['vehicleUID'] or '',
            'empCode': user_context['empCode']
        }
        
        db_url = f"{self.base_url}/api/MobileAppAction/InitiateDBCreation"
        self.log_request("POST", db_url, params)
        
        try:
            response = self.session.post(db_url, params=params, timeout=30)
            self.log_response(response)
            
            if response.status_code == 200:
                response_data = response.json()
                
                if response_data.get('IsSuccess', False):
                    logger.info("SUCCESS: Database creation initiated!")
                    logger.info(f"   Response: {json.dumps(response_data, indent=2)}")
                    return True
                else:
                    logger.error(f"ERROR: Database creation failed - {response_data.get('ErrorMessage', 'Unknown error')}")
                    return False
            else:
                logger.error(f"ERROR: Database creation failed with status {response.status_code}")
                return False
                
        except requests.exceptions.RequestException as e:
            logger.error(f"ERROR: Request failed - {str(e)}")
            return False
    
    def poll_database_creation_status(self, max_attempts: int = 30, poll_interval: int = 10) -> Optional[str]:
        """
        Step 3: Poll for database creation status
        Based on WINITDatabaseDownloader.monitorDBCreationStatus method
        """
        logger.info("STEP 3: Polling database creation status...")
        
        if not self.auth_token or not self.user_data:
            logger.error("ERROR: Authentication required before polling status")
            return None
        
        # Prepare user context for status check
        user_context = {
            "empUID": self.user_data.get("Emp", {}).get("UID"),
            "empCode": self.user_data.get("Emp", {}).get("Code"),
            "jobPositionUID": self.user_data.get("JobPosition", {}).get("UID"),
            "roleUID": self.user_data.get("Role", {}).get("UID"),
            "vehicleUID": None,  # Not provided in response
            "orgUID": self.user_data.get("JobPosition", {}).get("OrgUID"),
            "authToken": self.auth_token
        }
        
        # Use query parameters for status check (like React Native app)
        params = {
            'employeeUID': user_context['empUID'],
            'jobPositionUID': user_context['jobPositionUID'],
            'roleUID': user_context['roleUID'],
            'orgUID': user_context['orgUID'],
            'vehicleUID': user_context['vehicleUID'] or '',
            'empCode': user_context['empCode']
        }
        
        status_url = f"{self.base_url}/api/MobileAppAction/GetDBCreationStatus"
        
        for attempt in range(1, max_attempts + 1):
            logger.info(f"Status check attempt {attempt}/{max_attempts}")
            
            self.log_request("GET", status_url, params)
            
            try:
                response = self.session.get(status_url, params=params, timeout=30)
                self.log_response(response)
                
                if response.status_code == 200:
                    response_data = response.json()
                    
                    if response_data.get('IsSuccess', False):
                        status_data = response_data.get('Data', {})
                        status = status_data.get('Status')
                        progress = status_data.get('Progress', 0)
                        download_url = status_data.get('DownloadURL')
                        
                        logger.info(f"Status: {status}, Progress: {progress}%")
                        
                        if status == "Ready":
                            logger.info("SUCCESS: Database creation completed and ready for download!")
                            download_url = status_data.get('SqlitePath')
                            if download_url:
                                logger.info(f"Download URL: {download_url}")
                                return download_url
                            else:
                                logger.warning("WARNING: No download URL provided")
                                return None
                        elif status == "Failed":
                            logger.error("ERROR: Database creation failed on server")
                            return None
                        elif status == "InProgress":
                            logger.info(f"Database creation in progress ({progress}%)")
                            if attempt < max_attempts:
                                logger.info(f"Waiting {poll_interval} seconds before next check...")
                                time.sleep(poll_interval)
                            else:
                                logger.error("ERROR: Timeout waiting for database creation")
                                return None
                        elif status == "NotReady":
                            logger.info("Database not ready yet")
                            if attempt < max_attempts:
                                logger.info(f"Waiting {poll_interval} seconds before next check...")
                                time.sleep(poll_interval)
                            else:
                                logger.error("ERROR: Timeout waiting for database creation")
                                return None
                        else:
                            logger.warning(f"WARNING: Unknown status '{status}'")
                            if attempt < max_attempts:
                                time.sleep(poll_interval)
                    else:
                        logger.error(f"ERROR: Status check failed - {response_data.get('ErrorMessage', 'Unknown error')}")
                        return None
                else:
                    logger.error(f"ERROR: Status check failed with status {response.status_code}")
                    return None
                    
            except requests.exceptions.RequestException as e:
                logger.error(f"ERROR: Request failed - {str(e)}")
                return None
        
        logger.error("ERROR: Maximum polling attempts reached")
        return None
    
    def download_database(self, download_url: str) -> bool:
        """
        Step 4: Download database file (actual download)
        """
        logger.info("STEP 4: Downloading database file...")
        logger.info(f"Download URL: {download_url}")
        
        try:
            # Extract filename from the URL
            filename_from_url = download_url.split('/')[-1]
            logger.info(f"Extracted filename: {filename_from_url}")
            
            # Use the new download endpoint
            download_endpoint = f"{self.base_url}/api/MobileAppAction/DownloadZip/{filename_from_url}"
            logger.info(f"Using download endpoint: {download_endpoint}")
            
            # Download the actual file
            response = self.session.get(download_endpoint, timeout=60, stream=True)
            
            if response.status_code == 200:
                content_length = response.headers.get('content-length')
                content_type = response.headers.get('content-type', 'unknown')
                
                logger.info(f"SUCCESS: Database file download started")
                logger.info(f"   Content Length: {content_length} bytes")
                logger.info(f"   Content Type: {content_type}")
                
                # Save the file locally
                timestamp = int(time.time())
                filename = f"winit_database_{timestamp}.zip"
                
                logger.info(f"Downloading to: {filename}")
                
                with open(filename, 'wb') as f:
                    downloaded_bytes = 0
                    for chunk in response.iter_content(chunk_size=8192):
                        if chunk:
                            f.write(chunk)
                            downloaded_bytes += len(chunk)
                            
                            # Log progress every 1MB
                            if downloaded_bytes % (1024 * 1024) == 0:
                                mb_downloaded = downloaded_bytes // (1024 * 1024)
                                logger.info(f"   Downloaded: {mb_downloaded} MB")
                
                file_size = os.path.getsize(filename)
                logger.info(f"SUCCESS: Database file download completed!")
                logger.info(f"   File saved as: {filename}")
                logger.info(f"   File size: {file_size} bytes")
                
                return True
            else:
                logger.error(f"ERROR: Download failed with status {response.status_code}")
                logger.error(f"   Response: {response.text}")
                return False
                
        except requests.exceptions.RequestException as e:
            logger.error(f"ERROR: Download request failed - {str(e)}")
            return False
        except Exception as e:
            logger.error(f"ERROR: File save failed - {str(e)}")
            return False
    
    def run_complete_flow(self, user_id: str, password: str) -> bool:
        """
        Run the complete WINIT Mobile app login flow
        """
        logger.info("STARTING WINIT MOBILE APP LOGIN FLOW TEST")
        logger.info("=" * 80)
        logger.info(f"User ID: {user_id}")
        logger.info(f"Password: {'*' * len(password)}")
        logger.info(f"Start Time: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        logger.info("=" * 80)
        
        try:
            # Step 1: Authenticate user
            if not self.authenticate_user(user_id, password):
                logger.error("Authentication failed - stopping flow")
                return False
            
            # Step 2: Initiate database creation
            if not self.initiate_database_creation():
                logger.error("Database creation initiation failed - stopping flow")
                return False
            
            # Step 3: Poll for database creation status
            download_url = self.poll_database_creation_status()
            if not download_url:
                logger.error("Database creation status polling failed - stopping flow")
                return False
            
            # Step 4: Download database file
            if not self.download_database(download_url):
                logger.error("Database download failed - stopping flow")
                return False
            
            logger.info("SUCCESS: Complete WINIT Mobile app login flow completed successfully!")
            logger.info(f"End Time: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
            return True
            
        except Exception as e:
            logger.error(f"ERROR: Unexpected error during flow - {str(e)}")
            return False

def main():
    """Main function to run the test"""
    print("WINIT Mobile App Login Flow Test")
    print("=" * 50)
    
    # Configuration
    BASE_URL = "https://enables-cop-similarly-ozone.trycloudflare.com"
    USER_ID = "admin"
    PASSWORD = "password"
    
    print(f"Base URL: {BASE_URL}")
    print(f"User ID: {USER_ID}")
    print("=" * 50)
    
    # Create tester and run flow
    tester = WINITLoginFlowTester(BASE_URL)
    success = tester.run_complete_flow(USER_ID, PASSWORD)
    
    if success:
        print("\nSUCCESS: Test completed successfully!")
        print("Check 'login_flow_test.log' for detailed logs")
    else:
        print("\nFAILED: Test failed!")
        print("Check 'login_flow_test.log' for error details")

if __name__ == "__main__":
    main() 