import os
import shutil
import pprint


research = """
    <ResearchProjectDef>
        <defName>cosmeticImplants</defName>
        <label>Cosmetic Bio-implants</label>
        <description>Opens the possibility to create biological cosmetic implants. Tails, ears, and muzzles from a variety of animals are available for your colonists to fully express themselves.</description>
        <baseCost>1000</baseCost>
        <techLevel>Spacer</techLevel>
        <prerequisits>
            <li>Prosthetics</li>
        </prerequisits>
    </ResearchProjectDef>
    
    <ThingDef ParentName="BenchBase">
        <defName>TableCosmetics</defName>
        <label>cosmetics table</label>
        <description>A work station for growing cosmetic bioimplants.</description>
        <thingClass>Building_WorkTable</thingClass>
        <graphicData>
          <texPath>Things/Building/CosmeticsBench/CosmeticsBench/</texPath>
          <graphicClass>Graphic_Multi</graphicClass>
          <drawSize>(1.5,1.5)</drawSize>
        </graphicData>
        <castEdgeShadows>true</castEdgeShadows>
        <staticSunShadowHeight>0.20</staticSunShadowHeight>
        <costList>
          <Steel>150</Steel>
          <ComponentIndustrial>5</ComponentIndustrial>
        </costList>
        <altitudeLayer>Building</altitudeLayer>
        <fillPercent>0.5</fillPercent>
        <useHitPoints>True</useHitPoints>
        <statBases>
          <WorkToBuild>3000</WorkToBuild>
          <MaxHitPoints>180</MaxHitPoints>
          <Flammability>1.0</Flammability>
          <Cleanliness>-2</Cleanliness>
        </statBases>
        <size>(1,1)</size>
        <designationCategory>Production</designationCategory>
        <passability>PassThroughOnly</passability>
        <pathCost>70</pathCost>
        <hasInteractionCell>True</hasInteractionCell>
        <interactionCellOffset>(0,0,-1)</interactionCellOffset>
        <surfaceType>Item</surfaceType>
        <constructionSkillPrerequisite>4</constructionSkillPrerequisite>
        <inspectorTabs>
          <li>ITab_Bills</li>
        </inspectorTabs>
        <comps>
          <li Class="CompProperties_Power">
            <compClass>CompPowerTrader</compClass>
            <shortCircuitInRain>true</shortCircuitInRain>
            <basePowerConsumption>150</basePowerConsumption>
          </li>
          <li Class="CompProperties_Flickable"/>
          <li Class="CompProperties_AffectedByFacilities">
            <linkableFacilities>
              <li>ToolCabinet</li>
            </linkableFacilities>
          </li>
          <li Class="CompProperties_Breakdownable"/>
        </comps>
        <building>
          <spawnedConceptLearnOpportunity>BillsTab</spawnedConceptLearnOpportunity>
        </building>
        <constructEffect>ConstructMetal</constructEffect>
        <researchPrerequisites>
          <li>cosmeticImplants</li>
        </researchPrerequisites>
        <designationHotKey>Misc7</designationHotKey>
        <placeWorkers>
          <li>PlaceWorker_ShowFacilitiesConnections</li>
        </placeWorkers>
    </ThingDef>
"""

organicBase = """
    <ThingDef ParentName="OrganicProductBase">
        <defName>organicBase</defName>
        <label>Organic base</label>
        <description>A mix of animal and plant proteins, suitable to grow into a bioimplant around a plasteel frame given time and a skilled worker</description>
        <thingCategories>
            <li>ResourcesRaw</li>
        </thingCategories>
    </ThingDef>
    
	<RecipeDef>
		<defName>MakeOrganicBase</defName>
		<label>make organic base</label>
		<description>Make organic base suitable to grow bioimplants by mixing plant and animal matter.</description>
		<jobString>Making organic base.</jobString>
		<workAmount>300</workAmount>
		<workSpeedStat>CookSpeed</workSpeedStat>
		<effectWorking>Cook</effectWorking>
		<soundWorking>Recipe_CookMeal</soundWorking>
        <recipeUsers>
			<li>TableButcher</li>
		</recipeUsers>
		<allowMixingIngredients>true</allowMixingIngredients>
		<ingredients>
			<li>
				<filter>
					<categories>
						<li>MeatRaw</li>
					</categories>
				</filter>
				<count>1</count>
			</li>
			<li>
				<filter>
					<categories>
						<li>PlantFoodRaw</li>
                        <li>PlantMatter</li>
					</categories>
				</filter>
				<count>1</count>
			</li>
		</ingredients>
		<products>
			<organicBase>50</organicBase>
		</products>
		<fixedIngredientFilter>
            <categories>
				<li>PlantFoodRaw</li>
                <li>PlantMatter</li> 
            </categories>
		</fixedIngredientFilter>
		<defaultIngredientFilter>
			<categories>
				<li>Foods</li>
			</categories>
			<exceptedCategories>
				<li>EggsFertilized</li>
			</exceptedCategories>
			<exceptedThingDefs>
				<li>InsectJelly</li>
			</exceptedThingDefs>
		</defaultIngredientFilter>
		<workSkill>Cooking</workSkill>
        <researchPrerequisite>cosmeticImplants</researchPrerequisite>
	</RecipeDef>
"""

#1  defName
#2  Label
#3  description
#4  jobString
#5  Work amount
#6  part to install
#7  where it's installed
#8  hediff to apply
baseSurgery = """
    <RecipeDef ParentName="SurgeryFlesh">
        <defName>Install{}</defName>
        <label>graft {}</label>
        <description>{}</description>
        <workerClass>Recipe_InstallArtificialBodyPart</workerClass>
        <jobString>grafting on {}</jobString>
        <workAmount>{}</workAmount>
        <skillRequirements>
            <Medicine>7</Medicine>
        </skillRequirements>
        <ingredients>
            <li>
                <filter>
                    <categories>
                        <li>Medicine</li>
                    </categories>
                </filter>
            <count>3</count>
            </li>
            <li>
                <filter>
                    <thingDefs>
                        <li>{}</li>
                    </thingDefs>
                </filter>
                <count>1</count>
            </li>
        </ingredients>
        <fixedIngredientFilter>
            <categories>
                <li>Medicine</li>
            </categories>
            <thingDefs>
                <li>{}</li>
            </thingDefs>
        </fixedIngredientFilter>
        <appliedOnFixedBodyParts>
            <li>{}</li>
        </appliedOnFixedBodyParts>
        <addsHediff>{}</addsHediff>
        <recipeUsers>
			<li>Human</li>
		</recipeUsers>
		<dontShowIfAnyIngredientMissing>true</dontShowIfAnyIngredientMissing>
	</RecipeDef>
"""

#1  defName
#2  label insert
#3  description
#4  jobstring insert
#5  work amount
#6  organic base count
#7  plasteel count
#8  product defName
#9  product defName
baseRecipie = """
    <RecipeDef>
        <defName>create{}</defName>
        <label>Grow {}</label>
        <description>Grow a {}, able to be grafted on to a patient.</description>
        <jobString>growing {}</jobString>
        <workSkill>Crafting</workSkill>
        <workAmount>{}</workAmount>
        <skillRequirements>
            <Medicine>5</Medicine>
            <Crafting>9</Crafting>
        </skillRequirements>
        <recipeUsers>
            <li>TableCosmetics</li>
        </recipeUsers>
        <ingredients>
            <li>
                <filter>
                    <thingDefs>
                        <li>organicBase</li>
                    </thingDefs>
                </filter>
                <count>{}</count>
            </li>
            <li>
                <filter>
                    <thingDefs>
                        <li>Plasteel</li>
                    </thingDefs>
                </filter>
                <count>{}</count>
            </li>
        </ingredients>
        <fixedIngredientFilter>
            <thingDefs>
                <li>organicBase</li>
                <li>Plasteel</li>
            </thingDefs>
        </fixedIngredientFilter>
        <products>
            <{}>1</{}>
        </products>
        <researchPrerequisite>cosmeticImplants</researchPrerequisite>
    </RecipeDef>
"""
   

#1  defName
#2  Label
#3  description
baseThing = """
    <ThingDef ParentName="BodyPartArtificialBase">
        <defName>{}</defName>
        <label>{}</label>
        <description>{}</description>
        <graphicData>
			<texPath>Things/Item/Health/HealthItemProsthetic</texPath>
			<graphicClass>Graphic_Single</graphicClass>
			<drawSize>0.80</drawSize>
		</graphicData>
        <statBases>
            <Mass>3</Mass>
		</statBases>
		<techHediffsTags>
			<li>Simple</li>
		</techHediffsTags>
    </ThingDef>
"""

def fillThingDef(defName, label, description, part):
    if part == "Ear":
        label = label + " ear"
        description = "A {}.".format(label)
    elif (part == "Tail") or (part == "Jaw") or (part == "Foot") or (part == "Hand"):
            if label == "clawed":
                label = "cats claws"
                description = "A set of cats claws, with tissues to extend them on command"
            else:
                description = "A {}.".format(label)
    elif part == "Hand":
        description = "A {}. Distinctly lacking in thumbs."
    elif part == "Arm":
        if defName == "ThickFurLimb":
            label = "thick fur graft"
        elif defName == "FurredLimb":
            label = "fur graft"
        elif defName == "ScaledLimb":
            label == "scale graft"
        elif defName == "HideLimb":
            label == "hide graft"
        description = "A {}. The skin type will spread on it's own".format(label)
    elif (part == "Waist") or (part == "Pelvis"):
        description = "Necessary tissues for a working {}.".format(label) 
    elif part == "Leg":
        description = "Tissues needed to reform legs into a more animalistic form"
    elif label == "horns":
        description = "A pair of burly bull's horns"
    else:
        description = "No description"
    return baseThing.format(defName, label, description)
    
def fillRecipe(defName, label, workAmount, baseCost, plasteelCost, productDefName, part):
    if part == "Ear":
        label = label + " ear"
        description = "A {}.".format(label)
    elif (part == "Tail") or (part == "Jaw") or (part == "Foot") or (part == "Hand"):
        description = "A {}.".format(label)
    elif part == "Arm":
        if defName == "ThickFurLimb":
            label = "thick fur graft"
        elif defName == "FurredLimb":
            label = "fur graft"
        elif defName == "ScaledLimb":
            label == "scale graft"
        elif defName == "HideLimb":
            label == "hide graft"
        description = "Grow a {}. This will spread on it's own over the patient.".format(label)
    elif (part == "Waist") or (part == "Pelvis"):
        description = "Grow the necessary tissues for a working {}.".format(label) 
    elif part == "Leg":
        description = "Grow the tissues needed to reform a patients legs into a more animalistic form"
    elif label == "clawed":
        label = "Grow a set of cats claws"
    elif label == "horns":
        description = "Grow a pair of burly bull's horns"
    else:
        description = "No Description Yet"

    return baseRecipie.format(defName, label, description, label, workAmount, baseCost, plasteelCost, productDefName, productDefName)

def fillSurgery(defName, label, workAmount, defToInstall, part, hediffName):
    if part == "Ear":
        label = label + " ear"
        description = "Graft a {} to the patients head".format(label)
    elif part == "Eye":
        description = "Graft a {} to the patient.".format(label)
    elif part == "Head":
        description = "Graft a {} to the back of the patients head.".format(label)
    elif part == "Skull":
        if label == "thrumbohorn":
            description = "Graft a thrumbohorn to a patients face."
        else:
            description = "Graft a pair of {} to the top of the patients head.".format(label)
    elif part == "Jaw":
        if label == "venomous bite":
            description = "Graft a snakes jaw and venomous fangs to the patients face.".format(label)
        else:
            description = "Graft a {} to the patients face.".format(label)
    elif part == "Arm":
        if defName == "ThickFurLimb":
            label = "thick fur"
        elif defName == "FurredLimb":
            label = "fur"
        elif defName == "ScaledLimb":
            label == "scales"
        elif defName == "HideLimb":
            label == "hide"
        description = "Graft {} to a patients body. It will spread on it's own".format(label)
    elif part == "Hand":
        description = "Replace a patients hand with a {}.".format(label)
    elif part == "Leg":
        label = label  + "leg"
        description = "Reform a patients leg to a digitigrade form.".format(label)
    elif part == "Foot":
        description = "Replaces the patients foot with a {}.".format(label)
    elif part == "Waist":
        description = "Graft a {} to the patients waist.".format(label)
    elif part == "Tail":
        description = "Graft a {} to the patients tailbone.".format(label)
    else:
        description = "No description."
    return baseSurgery.format(defName, label, description, label, workAmount, defToInstall, defToInstall, part, hediffName)
    
    
#removes newlines and tabs 
def stripControls(list):
    for i in range(len(list)):
        #Strip newlines, and tabs
        list[i] = list[i].lstrip("\t")
        list[i] = list[i].rstrip("\n")
    return list

#strips XML data of enclosing tags
def stripTags(list):
    for i in range(len(list)):
        list[i] = list[i][list[i].find(">")+1:]
        list[i] = list[i][:list[i].find("</")]
    return list

#Takes morph_parts file path and the animal name, and extracts the def name, label, and description       
def extractHediffs(hediffPath):
    File = open(hediffPath)
    hediffList = File.readlines()
    File.close()

    hediffList = stripControls(hediffList)
    partsList = []
    
    found = 0
    for line in range(len(hediffList)):
        if hediffList[line].startswith("<HediffDef "):
            found = 0
            starts = line + 1
        elif hediffList[line].endswith("</description>") and found < 3:
            found = found + 1
        elif hediffList[line].endswith("</label>") and found < 3:
            found = found + 1
        if found == 2:
            found = 3
            ends = line+1
            part = hediffList[starts:ends]
            part.sort()
            partsList.append(stripTags(part))
        
    return partsList
    
#Takes the morph_full file path and the animal name, and extracts the part and hediff that affects that part
def extractAppliers(morphPath):
    File = open(morphPath)
    morphList = File.readlines()
    File.close()
    morphList = stripControls(morphList)
    
    install = "oops"
    partsList = []
    cleanList = []
    
    #filter xml to only mutation defs
    del morphList[:morphList.index("<li Class=\"Pawnmorph.HediffGiver_Mutation\">")-1]
    del morphList[morphList.index("</hediffGivers>"):]
    
    #Extract every def, from <li> to </li> for each morphs parts
    while True:
        try:
            partsList.append(morphList[morphList.index("<li Class=\"Pawnmorph.HediffGiver_Mutation\">"):morphList.index("</li>")+1])
            del morphList[morphList.index("<li Class=\"Pawnmorph.HediffGiver_Mutation\">"):morphList.index("</li>")+1]
        except ValueError:
            break
    #Run through each part, find the hediff, and part to affect
    for part in partsList:
        for line in part:
            if line.startswith("<hediff>"):
                hediff = line[8:-9]
            elif line.startswith("<partsToAffect>"):
                install = part[part.index(line)+1][4:-5]
        cleanList.append([hediff, install])
    return cleanList

def combine(parts, labels):
    #Take defName of each label, look for the matching defName in parts, append part to affect to label list.
    for i in range(len(labels)):
        for j in parts:
            if labels[i][0] == j[0]:
                labels[i].append(j[1])
    return labels
            
def extractData(Full, Parts):
    currentDefs = extractAppliers(Full) #[hediff, part]
    currentHediffs = extractHediffs(Parts) #[hediff, label, description]
    combined = combine(currentDefs, currentHediffs)
    return combined

def fillAndWrite(morphData, morphName):
    #Open and prepare files. Will overwrite the previous iteration.
    fileThings = open("..\\"+ morphName+ "_Things.xml","w")
    fileSurgery = open("..\\"+ morphName+ "_Surgery.xml","w")
    fileRecipe = open("..\\"+ morphName+ "_Recipie.xml","w")
    fileThings.write("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n<Defs>")
    fileSurgery.write("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n<Defs>")
    fileRecipe.write("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n<Defs>")
    #Run over each part in the data, and format them
    for i in range(len(morphData)):
        print(pprint.pformat(morphData[i]))
        fileThings.write(fillThingDef(morphData[i][0][5:], morphData[i][2], morphData[i][1], morphData[i][3]))
        fileRecipe.write(fillRecipe(morphData[i][0][5:],morphData[i][2], 350, 50, 20, morphData[i][0][5:], morphData[i][3]))
        fileSurgery.write(fillSurgery(morphData[i][0][5:], morphData[i][2], 500, morphData[i][0][5:], morphData[i][3], morphData[i][0]))
    #Close the XML, and close the files.
    fileThings.write("</Defs>\n")
    fileThings.close()
    fileSurgery.write("</Defs>\n")
    fileSurgery.close()
    fileRecipe.write("</Defs>\n")
    fileRecipe.close()
    
def main():
    mutations = "None"
    #Walk directory tree to find <animal>_Full and <animal>_Parts files
    for root, dirs, files in os.walk('.'):
        for file in files:
            if file.endswith("_Full.xml"):
                mutations = root + "\\" + file
            if file.endswith("_Parts.xml"):
                parts = root + "\\" + file
        #Only try to extract if data was found
        if mutations != "None":
            data = extractData(mutations, parts)
#            print(pprint.pformat(data))
            fillAndWrite(data, root[2:])



if __name__ == "__main__":
    main()  