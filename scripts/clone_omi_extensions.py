#!/usr/bin/env python3
"""
Clone and prepare the OMI glTF Extensions repository for analysis.
"""

import os
import logging
import tempfile
import shutil
from pathlib import Path
from typing import List, Optional
import subprocess

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

class OMIRepositoryManager:
    def __init__(self, temp_dir: Optional[str] = None):
        self.temp_dir = Path(temp_dir) if temp_dir else Path(tempfile.mkdtemp())
        self.repo_url = "https://github.com/omigroup/gltf-extensions.git"
        self.repo_path = self.temp_dir / "omi-gltf-extensions"
        
    def clone_omi_repository(self) -> bool:
        """Clone the OMI glTF Extensions repository."""
        try:
            logger.info(f"Cloning OMI glTF Extensions repository to {self.repo_path}")
            
            # Use shallow clone for faster download
            result = subprocess.run([
                'git', 'clone', '--depth', '1', 
                self.repo_url, str(self.repo_path)
            ], capture_output=True, text=True, timeout=300)
            
            if result.returncode != 0:
                logger.error(f"Git clone failed: {result.stderr}")
                return False
                
            logger.info("Repository cloned successfully")
            return True
            
        except subprocess.TimeoutExpired:
            logger.error("Git clone timed out")
            return False
        except Exception as e:
            logger.error(f"Error cloning repository: {e}")
            return False
    
    def validate_repository_structure(self) -> bool:
        """Validate that the cloned repository has the expected structure."""
        if not self.repo_path.exists():
            logger.error("Repository path does not exist")
            return False
            
        # Check for common directories/files that should exist
        expected_paths = [
            self.repo_path / "extensions",
            self.repo_path / "README.md"
        ]
        
        for path in expected_paths:
            if not path.exists():
                logger.warning(f"Expected path not found: {path}")
                
        # List what we actually found
        if (self.repo_path / "extensions").exists():
            extensions = list((self.repo_path / "extensions").iterdir())
            logger.info(f"Found {len(extensions)} extension directories")
        else:
            # Try to find extension directories in other locations
            extension_dirs = []
            for item in self.repo_path.rglob("*"):
                if item.is_dir() and item.name.startswith(("OMI_", "EXT_", "KHR_")):
                    extension_dirs.append(item)
            logger.info(f"Found {len(extension_dirs)} extension directories in repository")
            
        return True
    
    def list_extension_directories(self) -> List[Path]:
        """List all extension directories in the repository."""
        extension_dirs = []
        
        # Look for extension directories
        search_paths = [
            self.repo_path / "extensions",
            self.repo_path
        ]
        
        for search_path in search_paths:
            if not search_path.exists():
                continue
                
            for item in search_path.rglob("*"):
                if (item.is_dir() and 
                    (item.name.startswith(("OMI_", "EXT_", "KHR_")) or 
                     "extension" in item.name.lower()) and
                    item not in extension_dirs):
                    extension_dirs.append(item)
                    
        logger.info(f"Found {len(extension_dirs)} extension directories")
        return extension_dirs
    
    def get_repository_info(self) -> dict:
        """Get information about the cloned repository."""
        info = {
            'path': str(self.repo_path),
            'exists': self.repo_path.exists(),
            'commit_hash': None,
            'last_modified': None
        }
        
        if self.repo_path.exists():
            try:
                # Get current commit hash
                result = subprocess.run([
                    'git', 'rev-parse', 'HEAD'
                ], cwd=self.repo_path, capture_output=True, text=True)
                
                if result.returncode == 0:
                    info['commit_hash'] = result.stdout.strip()
                    
            except Exception as e:
                logger.warning(f"Could not get git info: {e}")
                
        return info
    
    def cleanup_temporary_files(self) -> None:
        """Clean up temporary files and directories."""
        if self.temp_dir.exists():
            try:
                shutil.rmtree(self.temp_dir)
                logger.info("Temporary files cleaned up")
            except Exception as e:
                logger.warning(f"Could not clean up temporary files: {e}")

def main():
    import argparse
    
    parser = argparse.ArgumentParser(description='Clone and analyze OMI glTF Extensions repository')
    parser.add_argument('--temp-dir', help='Temporary directory for cloning')
    parser.add_argument('--keep-files', action='store_true', help='Keep temporary files after analysis')
    
    args = parser.parse_args()
    
    manager = OMIRepositoryManager(args.temp_dir)
    
    try:
        if manager.clone_omi_repository():
            manager.validate_repository_structure()
            extension_dirs = manager.list_extension_directories()
            repo_info = manager.get_repository_info()
            
            print(f"Repository cloned to: {repo_info['path']}")
            print(f"Commit hash: {repo_info['commit_hash']}")
            print(f"Extension directories found: {len(extension_dirs)}")
            
            for ext_dir in extension_dirs[:10]:  # Show first 10
                print(f"  - {ext_dir.name}")
            if len(extension_dirs) > 10:
                print(f"  ... and {len(extension_dirs) - 10} more")
                
        else:
            print("Failed to clone repository")
            
    finally:
        if not args.keep_files:
            manager.cleanup_temporary_files()

if __name__ == '__main__':
    main()