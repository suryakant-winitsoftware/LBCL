import { useState, useCallback, useRef } from "react";
import { Organization, OrgType } from "@/services/organizationService";
import { 
  initializeOrganizationHierarchy, 
  handleOrganizationSelection, 
  getFinalSelectedOrganization,
  OrganizationLevel 
} from "@/utils/organizationHierarchyUtils";

export interface UseOrganizationHierarchyReturn {
  orgLevels: OrganizationLevel[];
  selectedOrgs: string[];
  initializeHierarchy: (organizations: Organization[], orgTypes: OrgType[]) => void;
  selectOrganization: (levelIndex: number, value: string) => void;
  resetHierarchy: () => void;
  finalSelectedOrganization: string | undefined;
  hasSelection: boolean;
}

export function useOrganizationHierarchy(): UseOrganizationHierarchyReturn {
  const [orgLevels, setOrgLevels] = useState<OrganizationLevel[]>([]);
  const [selectedOrgs, setSelectedOrgs] = useState<string[]>([]);
  const [organizations, setOrganizations] = useState<Organization[]>([]);
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([]);

  const orgLevelsRef = useRef<OrganizationLevel[]>([]);
  const selectedOrgsRef = useRef<string[]>([]);
  const organizationsRef = useRef<Organization[]>([]);
  const orgTypesRef = useRef<OrgType[]>([]);
  orgLevelsRef.current = orgLevels;
  selectedOrgsRef.current = selectedOrgs;
  organizationsRef.current = organizations;
  orgTypesRef.current = orgTypes;

  const initializeHierarchy = useCallback((orgs: Organization[], types: OrgType[]) => {
    setOrganizations(orgs);
    setOrgTypes(types);
    
    const initialLevels = initializeOrganizationHierarchy(orgs, types);
    setOrgLevels(initialLevels);
    setSelectedOrgs([]);
  }, []);

  const selectOrganization = useCallback((levelIndex: number, value: string) => {
    if (!value) return;
    
    const { updatedLevels, updatedSelectedOrgs } = handleOrganizationSelection(
      levelIndex,
      value,
      orgLevelsRef.current,
      selectedOrgsRef.current,
      organizationsRef.current,
      orgTypesRef.current
    );
    
    setOrgLevels(updatedLevels);
    setSelectedOrgs(updatedSelectedOrgs);
  }, []);
  const resetHierarchy = useCallback(() => {
    if (organizationsRef.current.length > 0 && orgTypesRef.current.length > 0) {
      const initialLevels = initializeOrganizationHierarchy(organizationsRef.current, orgTypesRef.current);
      setOrgLevels(initialLevels);
      setSelectedOrgs([]);
    }
  }, []);
  const finalSelectedOrganization = getFinalSelectedOrganization(selectedOrgs);
  const hasSelection = selectedOrgs.length > 0;

  return {
    orgLevels,
    selectedOrgs,
    initializeHierarchy,
    selectOrganization,
    resetHierarchy,
    finalSelectedOrganization,
    hasSelection
  };
}

export default useOrganizationHierarchy;