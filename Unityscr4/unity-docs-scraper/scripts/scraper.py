import requests
import json
import time
import os
import re
from bs4 import BeautifulSoup
from typing import Dict, List, Optional
import random

class UnityDocsScraper:
    def __init__(self, delay_range=(2, 5)):
        """
        Initialize the scraper with rate limiting
        
        Args:
            delay_range: Tuple of (min_delay, max_delay) in seconds between requests
        """
        self.base_url = "https://docs.unity3d.com/ScriptReference"
        self.delay_range = delay_range
        self.session = requests.Session()
        # Set a realistic user agent
        self.session.headers.update({
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
        })
        
    def get_class_page(self, class_name: str) -> Optional[str]:
        """
        Fetch a single Unity class documentation page

        Args:
            class_name: Name of the Unity class to fetch

        Returns:
            HTML content of the page or None if failed
        """
        # Handle different URL patterns for Unity documentation
        # Some classes might be in different namespaces or formats
        possible_urls = [
            f"{self.base_url}/{class_name}.html",  # Standard format
            f"{self.base_url}/{class_name}.html",  # Same as above but we'll try variations
        ]

        # For classes with dots (namespaces), we might need special handling
        if '.' in class_name:
            # Example: UnityEngine.Accessibility.AccessibilityHierarchy
            # Would become Accessibility.AccessibilityHierarchy
            normalized_name = class_name.replace('UnityEngine.', '')
            possible_urls.insert(0, f"{self.base_url}/{normalized_name}.html")

        for url in possible_urls:
            try:
                print(f"Fetching: {url}")
                response = self.session.get(url)

                # If we get a successful response, return it
                if response.status_code == 200:
                    # Apply rate limiting delay after each successful request
                    delay = random.uniform(*self.delay_range)
                    print(f"Waiting {delay:.2f}s before next request...")
                    time.sleep(delay)
                    return response.text
                elif response.status_code == 404:
                    print(f"URL not found: {url}")
                    continue  # Try the next URL
                else:
                    response.raise_for_status()  # Raise exception for other HTTP errors

            except requests.RequestException as e:
                print(f"Error fetching {url}: {e}")
                continue  # Try the next URL

        # If all URLs failed, apply delay and return None
        delay = random.uniform(*self.delay_range)
        time.sleep(delay)
        return None
    
    def parse_class_page(self, html_content: str, class_name: str) -> Dict:
        """
        Parse the HTML content of a Unity class page

        Args:
            html_content: Raw HTML content of the page
            class_name: Name of the class being parsed

        Returns:
            Dictionary containing extracted information
        """
        soup = BeautifulSoup(html_content, 'html.parser')

        # Extract class description
        description = ""
        # Look for the description in the first section after the heading
        desc_div = soup.find('div', class_='section')
        if desc_div:
            # Find the description paragraph after the heading
            h3_tags = desc_div.find_all('h3')
            for h3 in h3_tags:
                if h3.get_text().strip() == 'Description':
                    next_p = h3.find_next_sibling('p')
                    if next_p:
                        description = re.sub(r'\s+', ' ', next_p.get_text().strip())
                        break
            # If no 'Description' heading, try to get the first paragraph
            if not description:
                p_tags = desc_div.find_all('p')
                for p in p_tags:
                    if p.get_text().strip():
                        description = re.sub(r'\s+', ' ', p.get_text().strip())
                        break

        # Extract properties from the Properties table
        properties = []
        properties_header = soup.find('h3', string='Properties')
        if properties_header:
            properties_table = properties_header.find_next_sibling('table')
            if properties_table:
                rows = properties_table.find_all('tr')[1:]  # Skip header row
                for row in rows:
                    cells = row.find_all(['td', 'th'])
                    if len(cells) >= 2:
                        prop_name_link = cells[0].find('a')
                        if prop_name_link:
                            prop_name = prop_name_link.get_text(strip=True)
                        else:
                            prop_name = cells[0].get_text(strip=True)

                        prop_desc = cells[1].get_text(strip=True)
                        properties.append({
                            'name': prop_name,
                            'description': re.sub(r'\s+', ' ', prop_desc)
                        })

        # Extract methods from the Public Methods table
        methods = []
        methods_header = soup.find('h3', string='Public Methods')
        if methods_header:
            methods_table = methods_header.find_next_sibling('table')
            if methods_table:
                rows = methods_table.find_all('tr')[1:]  # Skip header row
                for row in rows:
                    cells = row.find_all(['td', 'th'])
                    if len(cells) >= 2:
                        method_name_link = cells[0].find('a')
                        if method_name_link:
                            method_name = method_name_link.get_text(strip=True)
                        else:
                            method_name = cells[0].get_text(strip=True)

                        method_desc = cells[1].get_text(strip=True)
                        methods.append({
                            'name': method_name,
                            'description': re.sub(r'\s+', ' ', method_desc)
                        })

        # Extract constructors if available
        constructors = []
        constructors_header = soup.find('h3', string='Constructors')
        if constructors_header:
            constructors_table = constructors_header.find_next_sibling('table')
            if constructors_table:
                rows = constructors_table.find_all('tr')[1:]  # Skip header row
                for row in rows:
                    cells = row.find_all(['td', 'th'])
                    if len(cells) >= 2:
                        constructor_name_link = cells[0].find('a')
                        if constructor_name_link:
                            constructor_name = constructor_name_link.get_text(strip=True)
                        else:
                            constructor_name = cells[0].get_text(strip=True)

                        constructor_desc = cells[1].get_text(strip=True)
                        constructors.append({
                            'name': constructor_name,
                            'description': re.sub(r'\s+', ' ', constructor_desc)
                        })

        # Look for code examples
        examples = []
        # Unity docs often have code examples in pre tags
        pre_tags = soup.find_all('pre')
        for pre in pre_tags:
            code_text = pre.get_text()
            if len(code_text.strip()) > 20:  # Filter out very short snippets
                examples.append(code_text.strip())

        return {
            'class_name': class_name,
            'description': description,
            'properties': properties,
            'methods': methods,
            'constructors': constructors,
            'examples': examples,
            'url': f"{self.base_url}/{class_name}.html"
        }
    
    def scrape_class(self, class_name: str) -> Optional[Dict]:
        """
        Scrape a single Unity class
        
        Args:
            class_name: Name of the Unity class to scrape
            
        Returns:
            Dictionary containing class information or None if failed
        """
        html_content = self.get_class_page(class_name)
        if html_content:
            return self.parse_class_page(html_content, class_name)
        return None
    
    def scrape_multiple_classes(self, class_names: List[str], batch_size: int = 100) -> List[Dict]:
        """
        Scrape multiple Unity classes in batches
        
        Args:
            class_names: List of class names to scrape
            batch_size: Number of classes to scrape before saving
            
        Returns:
            List of dictionaries containing class information
        """
        all_data = []
        total_classes = len(class_names)
        
        for i, class_name in enumerate(class_names):
            print(f"Processing {i+1}/{total_classes}: {class_name}")
            
            class_data = self.scrape_class(class_name)
            if class_data:
                all_data.append(class_data)
                print(f"  Successfully scraped {class_name}")
            else:
                print(f"  Failed to scrape {class_name}")
        
        return all_data
    
    def save_batch(self, data: List[Dict], batch_num: int, output_dir: str = "data"):
        """
        Save a batch of scraped data to a JSON file
        
        Args:
            data: List of class data dictionaries
            batch_num: Batch number for filename
            output_dir: Directory to save the batch file
        """
        os.makedirs(output_dir, exist_ok=True)
        filename = os.path.join(output_dir, f"batch_{batch_num:03d}.json")
        
        with open(filename, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
        
        print(f"Saved batch {batch_num} with {len(data)} classes to {filename}")


def main():
    # Read the list of classes from the file created earlier
    if not os.path.exists('unity_engine_classes.txt'):
        print("Error: unity_engine_classes.txt not found. Run parse_toc.py first.")
        return
    
    with open('unity_engine_classes.txt', 'r', encoding='utf-8') as f:
        class_names = [line.strip() for line in f if line.strip()]
    
    print(f"Loaded {len(class_names)} classes to scrape")
    
    # Initialize scraper with rate limiting (2-5 seconds between requests)
    scraper = UnityDocsScraper(delay_range=(2, 5))
    
    # For testing, let's just scrape the first 5 classes
    test_classes = class_names[:5]
    print(f"Scraping first {len(test_classes)} classes for testing...")
    
    scraped_data = scraper.scrape_multiple_classes(test_classes)
    
    # Save the test batch
    scraper.save_batch(scraped_data, 1)
    
    print(f"Completed scraping {len(scraped_data)} out of {len(test_classes)} test classes")


if __name__ == "__main__":
    main()