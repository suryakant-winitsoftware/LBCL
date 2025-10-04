#!/usr/bin/env python3
"""
Test promotion data sync with different OrgUID parameter values
"""
import psycopg2
from psycopg2.extras import RealDictCursor
import os
import json

def test_promotion_sync():
    """Test promotion data retrieval with different OrgUID values"""
    
    # Database connection parameters
    config = {
        'host': 'localhost',
        'database': 'winit_development',
        'user': 'postgres',
        'password': 'password'
    }
    
    try:
        conn = psycopg2.connect(**config)
        cursor = conn.cursor(cursor_factory=RealDictCursor)
        
        # The actual masterdata_query from table_group_entity
        query = """
        SELECT id, uid, company_uid, org_uid, code, name, category, has_slabs, 
               valid_from, valid_upto, is_active, display_name, priority, 
               created_time, modified_time, server_add_time, server_modified_time, 
               created_by, modified_by, level, format, order_type, description, 
               max_deal_count, max_discount_amount, selection_model, 
               multi_product_enabled, apply_for_excluded_items, is_approval_created, 
               status, format_label, min_invoice_value, max_invoice_value, 
               min_total_quantity, max_total_quantity, min_line_count, 
               max_line_count, min_brand_count, max_brand_count, is_unconditional, 
               invoice_discount_type, invoice_discount_amount, invoice_discount_percentage, 
               ss, has_fact_sheet, type, product_selection_type, product_selection_data, 
               dynamic_product_selection, dynamic_hierarchy_selections, promo_message, 
               internal_remarks 
        FROM promotion 
        WHERE (%(OrgUID)s IS NULL OR org_uid = %(OrgUID)s)
        """
        
        # Test with None (NULL)
        print("[INFO] Testing with OrgUID = None (NULL)")
        cursor.execute(query, {'OrgUID': None})
        results_null = cursor.fetchall()
        print(f"[INFO] Records returned with NULL OrgUID: {len(results_null)}")
        
        # Test with empty string
        print("[INFO] Testing with OrgUID = '' (empty string)")
        cursor.execute(query, {'OrgUID': ''})
        results_empty = cursor.fetchall()
        print(f"[INFO] Records returned with empty OrgUID: {len(results_empty)}")
        
        # Test with a specific org_uid that exists
        print("[INFO] Testing with OrgUID = 'FR001' (specific value)")
        cursor.execute(query, {'OrgUID': 'FR001'})
        results_fr001 = cursor.fetchall()
        print(f"[INFO] Records returned with FR001 OrgUID: {len(results_fr001)}")
        
        # Show what org_uid values exist in the promotion table
        cursor.execute("SELECT DISTINCT org_uid, COUNT(*) FROM promotion GROUP BY org_uid")
        org_distribution = cursor.fetchall()
        print(f"[INFO] Org UID distribution in promotion table:")
        for row in org_distribution:
            org_uid = row['org_uid'] if row['org_uid'] is not None else 'NULL'
            count = row['count']
            print(f"  {org_uid}: {count} records")
        
        # Test the exact query structure that would be used in C#
        # In C# Dictionary<string, object> with null values might be handled differently
        print("[INFO] Testing query structure for C# compatibility")
        
        if results_null:
            sample_record = dict(results_null[0])
            print("[INFO] Sample record from NULL query:")
            # Show key fields
            key_fields = ['uid', 'name', 'org_uid', 'description', 'format']
            for field in key_fields:
                if field in sample_record:
                    print(f"  {field}: {sample_record[field]}")
        
        conn.close()
        
    except Exception as e:
        print(f"[ERROR] Failed to test promotion sync: {e}")

if __name__ == "__main__":
    test_promotion_sync()