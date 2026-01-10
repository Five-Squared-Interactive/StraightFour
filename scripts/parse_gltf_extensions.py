#!/usr/bin/env python3
"""
Parse OMI glTF extension specifications and extract metadata.
"""

import json
import logging
import re
from pathlib import Path
from typing import Dict, List, Any, Optional
import yaml

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

class GLTFExtensionParser:
    def __init__(self, repository_path: str):
        self.repo_path = Path(repository_path)
        self.extension_catalog = {}
        
    def scan_extension_directories(self) -> List[Path]:
        """Scan for extension directories."""
        extension_dirs = []
        
        # Look for extension directories
        for item in self.repo_path.rglob("*"):
            if (item.is_dir() and 
                (item.name.startswith(("OMI_", "EXT_", "KHR_")) or
                 self._looks_like_extension_dir(item))):
                extension_dirs.append(item)
                
        logger.info(f"Found {len(extension_dirs)} extension directories")
        return extension_dirs
    
    def _looks_like_extension_dir(self, path: Path) -> bool:
        """Check if directory looks like an extension directory."""
        # Check if it contains schema files or README
        has_schema = any(f.name.lower().endswith(('.json', '.yaml', '.yml')) 
                        for f in path.iterdir() if f.is_file())
        has_readme = any(f.name.lower().startswith('readme') 
                        for f in path.iterdir() if f.is_file())
        
        return has_schema or has_readme
    
    def parse_schema_files(self, extension_dir: Path) -> Dict[str, Any]:
        """Parse JSON schema files in an extension directory."""
        schema_info = {
            'properties': {},
            'required': [],
            'definitions': {}
        }
        
        schema_files = list(extension_dir.glob("*.json")) + list(extension_dir.glob("*.yaml")) + list(extension_dir.glob("*.yml"))
        
        for schema_file in schema_files:
            try:
                if schema_file.suffix.lower() == '.json':
                    with open(schema_file, 'r', encoding='utf-8') as f:
                        schema_data = json.load(f)
                else:
                    with open(schema_file, 'r', encoding='utf-8') as f:
                        schema_data = yaml.safe_load(f)
                
                # Extract schema information
                if isinstance(schema_data, dict):
                    if 'properties' in schema_data:
                        schema_info['properties'].update(schema_data['properties'])
                    if 'required' in schema_data:
                        schema_info['required'].extend(schema_data['required'])
                    if 'definitions' in schema_data:
                        schema_info['definitions'].update(schema_data['definitions'])
                        
            except Exception as e:
                logger.warning(f"Error parsing schema file {schema_file}: {e}")
                
        return schema_info
    
    def parse_readme_files(self, extension_dir: Path) -> Dict[str, Any]:
        """Parse README files for extension information."""
        readme_info = {
            'title': extension_dir.name,
            'description': '',
            'features': [],
            'examples': [],
            'dependencies': []
        }
        
        readme_files = [f for f in extension_dir.iterdir() 
                       if f.is_file() and f.name.lower().startswith('readme')]
        
        for readme_file in readme_files:
            try:
                with open(readme_file, 'r', encoding='utf-8') as f:
                    content = f.read()
                    
                readme_info.update(self._parse_readme_content(content))
                
            except Exception as e:
                logger.warning(f"Error parsing README {readme_file}: {e}")
                
        return readme_info
    
    def _parse_readme_content(self, content: str) -> Dict[str, Any]:
        """Parse README content and extract structured information."""
        info = {
            'title': '',
            'description': '',
            'features': [],
            'examples': [],
            'dependencies': []
        }
        
        lines = content.split('\n')
        current_section = None
        
        for line in lines:
            line = line.strip()
            
            # Extract title (first # heading)
            if line.startswith('# ') and not info['title']:
                info['title'] = line[2:].strip()
                continue
                
            # Identify sections
            if line.startswith('## '):
                current_section = line[3:].lower().strip()
                continue
                
            # Extract description (first paragraph)
            if not info['description'] and line and not line.startswith('#'):
                info['description'] = line
                continue
                
            # Extract features from lists
            if current_section in ['features', 'supported', 'capabilities'] and line.startswith('- '):
                info['features'].append(line[2:].strip())
                
            # Extract dependencies
            if ('depend' in current_section if current_section else False) and line.startswith('- '):
                info['dependencies'].append(line[2:].strip())
                
        return info
    
    def extract_extension_metadata(self, extension_dir: Path) -> Dict[str, Any]:
        """Extract complete metadata for an extension."""
        metadata = {
            'name': extension_dir.name,
            'path': str(extension_dir),
            'schema': {},
            'readme': {},
            'files': []
        }
        
        # List all files in the extension directory
        for file_path in extension_dir.iterdir():
            if file_path.is_file():
                metadata['files'].append(file_path.name)
        
        # Parse schema files
        metadata['schema'] = self.parse_schema_files(extension_dir)
        
        # Parse README files
        metadata['readme'] = self.parse_readme_files(extension_dir)
        
        # Determine extension type and purpose
        metadata['purpose'] = self._determine_extension_purpose(metadata)
        metadata['category'] = self._categorize_extension(metadata)
        
        return metadata
    
    def _determine_extension_purpose(self, metadata: Dict[str, Any]) -> str:
        """Determine the purpose of an extension based on its metadata."""
        name = metadata['name'].lower()
        description = metadata['readme'].get('description', '').lower()
        
        # Common extension purposes
        if any(keyword in name for keyword in ['physics', 'body', 'collision']):
            return 'physics'
        elif any(keyword in name for keyword in ['audio', 'sound', 'emitter']):
            return 'audio'
        elif any(keyword in name for keyword in ['light', 'lighting', 'punctual']):
            return 'lighting'
        elif any(keyword in name for keyword in ['material', 'pbr', 'texture']):
            return 'material'
        elif any(keyword in name for keyword in ['animation', 'motion', 'keyframe']):
            return 'animation'
        elif any(keyword in name for keyword in ['vehicle', 'seat', 'spawn']):
            return 'vehicle'
        elif any(keyword in name for keyword in ['behavior', 'script', 'trigger']):
            return 'behavior'
        else:
            return 'unknown'
    
    def _categorize_extension(self, metadata: Dict[str, Any]) -> str:
        """Categorize extension by vendor/organization."""
        name = metadata['name']
        
        if name.startswith('OMI_'):
            return 'OMI'
        elif name.startswith('EXT_'):
            return 'Multi-vendor'
        elif name.startswith('KHR_'):
            return 'Khronos'
        else:
            return 'Unknown'
    
    def generate_extension_catalog(self, extension_dirs: List[Path]) -> Dict[str, Any]:
        """Generate complete extension catalog."""
        logger.info("Starting OMI glTF extension analysis...")
        
        extensions = {}
        
        for ext_dir in extension_dirs:
            try:
                extension_metadata = self.extract_extension_metadata(ext_dir)
                extensions[extension_metadata['name']] = extension_metadata
                logger.info(f"Analyzed extension: {extension_metadata['name']}")
                
            except Exception as e:
                logger.error(f"Error analyzing extension {ext_dir}: {e}")
        
        catalog = {
            'metadata': {
                'total_extensions': len(extensions),
                'categories': list(set(e['category'] for e in extensions.values())),
                'purposes': list(set(e['purpose'] for e in extensions.values())),
                'analysis_timestamp': self._get_timestamp()
            },
            'extensions': extensions
        }
        
        self.extension_catalog = catalog
        logger.info(f"Analyzed {len(extensions)} extensions")
        return catalog
    
    def _get_timestamp(self) -> str:
        """Get current timestamp."""
        from datetime import datetime
        return datetime.now().isoformat()
    
    def save_catalog(self, output_path: str) -> None:
        """Save extension catalog to JSON file."""
        with open(output_path, 'w') as f:
            json.dump(self.extension_catalog, f, indent=2)
        logger.info(f"Extension catalog saved to {output_path}")

def main():
    import argparse
    
    parser = argparse.ArgumentParser(description='Parse OMI glTF extensions')
    parser.add_argument('repository_path', help='Path to cloned OMI glTF Extensions repository')
    parser.add_argument('--output', default='extension_catalog.json', help='Output file for extension catalog')
    
    args = parser.parse_args()
    
    parser_obj = GLTFExtensionParser(args.repository_path)
    extension_dirs = parser_obj.scan_extension_directories()
    catalog = parser_obj.generate_extension_catalog(extension_dirs)
    parser_obj.save_catalog(args.output)
    
    print(f"Found {catalog['metadata']['total_extensions']} extensions")
    print(f"Categories: {', '.join(catalog['metadata']['categories'])}")
    print(f"Purposes: {', '.join(catalog['metadata']['purposes'])}")

if __name__ == '__main__':
    main()