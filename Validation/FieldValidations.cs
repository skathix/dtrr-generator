using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Tools.Validation;

public class FieldValidations
{
    private void MbiCheck(string beneficiary_id)
    {
        var regex = @"^\d[A-Za-z]\d\d[A-Za-z]\d\d[A-Za-z]{2}\d$";
        var match = Regex.Match(beneficiary_id, regex
            , RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            Console.WriteLine(
                "MBI format: you would get a TCR 007 - Reject for this");
        }

        Console.WriteLine(
            "POTENTIAL MBI not found: you would get a TCR 008 - Reject for this");
    }

    private void NamesCheck(string LastName, string FirstName, string MI)
    {
        var surnameRegex = @"^[A-Za-z][a-zA-z\\s]{11}$";
        var firstNameRegex = @"^[A-Za-z][A-Za-z]{6}$";
        var miRegex = @"^[a-zA-z\\s]+$";

        var surnamematch = Regex.Match(LastName, surnameRegex
            , RegexOptions.IgnoreCase);
        if (!surnamematch.Success)
        {
            Console.WriteLine(
                "Required item - you will get a reject for this record if 3 of 4 don't match");
        }


        var firstmatch = Regex.Match(FirstName, firstNameRegex
            , RegexOptions.IgnoreCase);
        if (!firstmatch.Success)
        {
            Console.WriteLine(
                "Required item - you will get a reject for this record if 3 of 4 don't match");
        }

        var mimatch = Regex.Match(MI, miRegex
            , RegexOptions.IgnoreCase);
        if (!mimatch.Success)
        {
            Console.WriteLine(
                "Required item - you will get a reject for this record if 3 of 4 don't match");
        }
    }

    void SexCheck(string sexCode)
    {
        switch (sexCode)
        {
            case "1":
                Console.WriteLine("Male");
                break;
            case "2":
                Console.WriteLine("Female");
                break;
            default:
                Console.WriteLine(
                    "This will be set to 0: not fail nor rejection");
                break;
        }
    }

    private void ValidateBirthDate(string date)
    {
        var dateRegex = @"^\d{4}(0[1-9]|1[012])(0[1-9]|[12][0-9]|3[01])$";
        var match = Regex.Match(date, dateRegex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Effective Date");
        }

        var yearText = date.Substring(0, 4);
        var year = int.Parse(yearText);
        var day = date.Substring(6, 2);
        if (year < 1870)
        {
            Console.WriteLine(
                "This is a date before 1870 - will be rejected");
        }

        if (year >= (DateTime.Now.Year) + 1)
        {
            Console.WriteLine(
                "This is a date too far in the future - will be rejected");
        }
    }

    void recordTypeHardcode(string recordType)
    {
        var HcRecordType = ("T");
    }

    private void contract(string contractNumber)
    {
        var regex = @"^[A-Za-z]\d\d\d\d$";
        var match = Regex.Match(contractNumber, regex
            , RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            Console.WriteLine(
                "Contract Number format: you would get a TCR 007 - Reject for this");
        }
    }

    private void NumericStateCode(string StateCode)
    {
        var regex = @"^\d\d$";
        var match = Regex.Match(StateCode, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "State Code: Sent by CMS");
        }
    }

    private void NumericCountyCode(string CountyCode)
    {
        var regex = @"^\d\d\d$";
        var match = Regex.Match(CountyCode, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "County Code: Sent by CMS");
        }
    }

    private void DisabilityCode(int DisabilityIndicator)
    {
        switch (DisabilityIndicator)
        {
            case 0:
                Console.WriteLine("No Disability");
                break;
            case 1:
                Console.WriteLine("Disabled without ERSD");
                break;
            case 2:
                Console.WriteLine("ESRD Only");
                break;
            case 3:
                Console.WriteLine("Disabled with ESRD");
                break;
            default:
                Console.WriteLine("NA");
                break;
        }
    }

    void hospiceCheck(string hospice)
    {
        switch (hospice)
        {
            case "0":
                Console.WriteLine("No Hospice");
                break;
            case "1":
                Console.WriteLine("Hospice");
                break;
            default:
                Console.WriteLine("Not applicable");
                break;
        }
    }

    private void InstitutionalCode(int institutionalIndicator)
    {
        switch (institutionalIndicator)
        {
            case 0:
                Console.WriteLine("No Institutional");
                break;
            case 1:
                Console.WriteLine("Institutional");
                break;
            case 2:
                Console.WriteLine("NHC");
                break;
            case 3:
                Console.WriteLine("HCBS");
                break;
            default:
                Console.WriteLine("NA");
                break;
        }
    }

    void esrdCheck(string esrd)
    {
        switch (esrd)
        {
            case "0":
                Console.WriteLine("No End-Stage Renal Disease");
                break;
            case "1":
                Console.WriteLine("End Stage Renal Disease");
                break;
            default:
                Console.WriteLine("Not applicable");
                break;
        }
    }

    void TransactionReply(string transactionReply)
    {
        var regex = @"^\d\d\d$";
        var match = Regex.Match(transactionReply, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Non-numeric TRC");
        }
    }

    void SentTransactionCode(string transactionCode)
    {
        var regex = @"^\d\d$";
        var match = Regex.Match(transactionCode, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Non-numeric Transaction Code");
        }

        switch (transactionCode)
        {
            case "51":
                Console.WriteLine("No End-Stage Renal Disease");
                break;
            case "54":
                Console.WriteLine("End Stage Renal Disease");
                break;
            case "61":
                Console.WriteLine("No End-Stage Renal Disease");
                break;
            case "72":
                Console.WriteLine("End Stage Renal Disease");
                break;
            case "76":
                Console.WriteLine("No End-Stage Renal Disease");
                break;
            case "79":
                Console.WriteLine("End Stage Renal Disease");
                break;
            case "80":
                Console.WriteLine("No End-Stage Renal Disease");
                break;
            case "81":
                Console.WriteLine("End Stage Renal Disease");
                break;
            case "82":
                Console.WriteLine("No End-Stage Renal Disease");
                break;
            case "83":
                Console.WriteLine("End Stage Renal Disease");
                break;
            case "90":
                Console.WriteLine("No End-Stage Renal Disease");
                break;
            case "91":
                Console.WriteLine("End Stage Renal Disease");
                break;
            case "92":
                Console.WriteLine("No End-Stage Renal Disease");
                break;
            case "93":
                Console.WriteLine("End Stage Renal Disease");
                break;
            case "94":
                Console.WriteLine("End Stage Renal Disease");
                break;
            case "95":
                Console.WriteLine("No End-Stage Renal Disease");
                break;

            default:
                Console.WriteLine("Not applicable");
                break;
        }
    }

    void EntitlementCheck(string entitlementCheck)
    {
        var regex = @"^\[YZ/s]$";
        var match = Regex.Match(entitlementCheck, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Transaction Code");
        }
    }

    void EffectiveDateCheck(string effectiveDateCheck, string transactionReply)
    {
        var dateRegex = @"^\d{4}(0[1-9]|1[012])(0[1-9]|[12][0-9]|3[01])$";
        var match = Regex.Match(effectiveDateCheck, dateRegex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Effective Date");
        }

        switch (transactionReply)
        {
            case "071":
                Console.WriteLine("Effective date of the hospice period");
                break;
            case "072":
                goto case "071";
            case "090":
                Console.WriteLine("Current Calendar Month");
                break;
            case "091":
                Console.WriteLine("Previously Reported incorrect death date");
                break;
            case "121":
                Console.WriteLine("PBP Effective Enrollment");
                break;
            case "194":
                goto case "121";
            case "223":
                goto case "121";
            case "245":
                Console.WriteLine("Payments impacted by MSP date");
                break;
            case "280":
                goto case "245";
            //this needs additional verification
            case "293":
                Console.WriteLine(
                    "Enrollment End Date - last day of the month ");
                break;
            case "305":
                Console.WriteLine("New ZIP Code Start Date");
                break;
            case "345":
                Console.WriteLine("Effective date of attempted enrollment");
                break;
            case "346":
                Console.WriteLine("End date of the enrollment period");
                break;
            case "347":
                Console.WriteLine("Start date of the reenrollment period");
                break;
            case "366":
                Console.WriteLine(
                    "Effective date for change of Medicaid status");
                break;
            case "368":
                Console.WriteLine(
                    "Plans payments impacted date - based on MSP date");
                break;
            case "409":
                Console.WriteLine("Effective date for M3P");
                break;
            case "410":
                goto case "409";
            case "411":
                goto case "409";
            case "412":
                goto case "409";
            case "413":
                goto case "409";
            case "414":
                goto case "409";
            case "415":
                goto case "409";
            case "416":
                goto case "409";
            case "417":
                goto case "409";
            case "701":
                Console.WriteLine("New Enrollment period start date");
                break;
            case "702":
                Console.WriteLine("Fill-in enrollment period start date");
                break;
            case "703":
                Console.WriteLine("Start date of cancelled enrollment period");
                break;
            case "704":
                Console.WriteLine(
                    "Start date of enrollment cancelled for PBP correction");
                break;
            case "705":
                Console.WriteLine(
                    "Start date of enrollment for PBP correction");
                break;
            case "706":
                Console.WriteLine(
                    "Start date of enrollment cancelled for segment correction");
                break;
            case "707":
                Console.WriteLine(
                    "Start date of enrollment for segment correction");
                break;
            case "708":
                Console.WriteLine(
                    "Enrollment period end date assigned to opened ended enrollment");
                break;
            case "709":
                Console.WriteLine("New start date resulting from update");
                break;
            case "710":
                goto case "709";
            case "711":
                Console.WriteLine("New end date resulting from update");
                break;
            case "712":
                goto case "711";
            case "713":
                Console.WriteLine("End date removed");
                break;
            default:
                Console.WriteLine("Not applicable");
                break;
        }
    }

    void WaHardcode(string WorkingAgeIndicator)
    {
        var workingAgeCheck = ("1");
    }

    void PbpIdCode(string PlanBenefitPackageId)
    {
        var regex = @"^\d\d\d$";
        var match = Regex.Match(PlanBenefitPackageId, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid PBP Number");
        }

    }

    void dtrrfiller(string dtrrFiller)
    {
        var regex = @"^\s$";
        var match = Regex.Match(dtrrFiller, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid filler");
        }

    }

    void TransactionDate(string transactionDate, string transactionReply)
    {
        var dateRegex = @"^\d{4}(0[1-9]|1[012])(0[1-9]|[12][0-9]|3[01])$";
        var match = Regex.Match(transactionDate, dateRegex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Effective Date");
        }

        switch (transactionReply)
        {
            case "121":
                goto case "223";
            case "072":
                goto case "223";
            case "223":
                Console.WriteLine("Date report was generated");
                break;
            default:
                Console.WriteLine("Date of Transaction");
                break;
        }
    }

    void UiInitiatedChangeCode(string UIInitiatedChangeFlag)
    {
        var regex = @"^\d$";
        var match = Regex.Match(UIInitiatedChangeFlag, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid PBP Number");
        }

        switch (UIInitiatedChangeFlag)
        {
            case "0":
                Console.WriteLine("Transaction not created in user interface");
                break;
            case "1":
                Console.WriteLine("Transaction created in user interface");
                break;
            default:
                Console.WriteLine("Not applicable");
                break;
        }
    }
    
    void todoVariableData(string VariableData)
    {
        var regex = @"^\d$";
        var match = Regex.Match(VariableData, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "WTF?!");
        }

        switch (VariableData)
        {
            
            default:
                Console.WriteLine("Not applicable");
                break;
        }
    }
    
    void districtOfficeCode(string DistrictOfficeCode)
    {
        var regex = @"^\s\s\s$";
        var match = Regex.Match(DistrictOfficeCode, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid unless the transaction is a 53");
        }

    }
    private void prevPartDContractAndPBP(string PrevPartDContractAndPBP)
    {
        var empty = @"^[/s{8}$";
        var regex = @"^[A-Za-z]\d\d\d\d\d\d\d$";
        var emptyMatch = Regex.Match(PrevPartDContractAndPBP, empty
            , RegexOptions.IgnoreCase);
        if (!emptyMatch.Success)
        {
            var match = Regex.Match(PrevPartDContractAndPBP, regex
                , RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                Console.WriteLine(
                    "Contract Number/PBP format error");
            }
        }
    }
    
    void SepReasonCode(string sepReasonCode, string electionType, string transactionReply)
    {
       var type1 = "S";
       var type2 = "Y";
       if (electionType != type1)
       {
           if (electionType != type2)
           {
               Console.WriteLine("This string should be blank.");
           }
       }
        var sepRegex = @"^[a-zA-Z0-9]{2}$";
        var match = Regex.Match(sepReasonCode, sepRegex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid SEP entry");
        }

        switch (transactionReply)
        {
            case "011":
                goto case "725";
            case "013":
                goto case "725";
            case "015":
                goto case "725";
            case "018":
                goto case "725";
            case "022":
                goto case "725";
            case "023":
                goto case "725";
            case "025":
                goto case "725";
            case "026":
                goto case "725";
            case "100":
                goto case "725";
            case "397":
                goto case "725";
            case "401":
                goto case "725";
            case "402":
                goto case "725";
            case "701":
                goto case "725";
            case "702":
                goto case "725";
            case "704":
                goto case "725";
            case "705":
                goto case "725";
            case "708":
                goto case "725";
            case "709":
                goto case "725";
            case "710":
                goto case "725";
            case "711":
                goto case "725";
            case "712":
                goto case "725";
            case "713":
                goto case "725";
            case "717":
                goto case "725";
          
            case "725":
                Console.WriteLine("Should not be blank");
                break;
            default:
                Console.WriteLine("Should be blank");
                break;
        }
    }
    void DtrrFiller1(string dtrrFiller1)
    {
        var regex = @"^\s{6}$";
        var match = Regex.Match(dtrrFiller1, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid filler");
        }
    }
    
    private void SourceId(string sourceId)
    {
        var regex = @"^[A-Za-z]\d\d\d\d$";
        var match = Regex.Match(sourceId, regex
            , RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            Console.WriteLine(
                "Source Id Error: should match contract number");
        }
    }
    
    void PriorPlanBenefitPackageId(string priorPlanBenefitPackageId)
    {
        var regex = @"^\d\d\d$";
        var match = Regex.Match(priorPlanBenefitPackageId, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid PBP Number");
        }

    }

    void ApplicationDate(string applicationDate)
    {
        var dateRegex = @"^\d{4}(0[1-9]|1[012])(0[1-9]|[12][0-9]|3[01])$";
        var match = Regex.Match(applicationDate, dateRegex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Application Date");
        }

        Console.WriteLine(
            "This should be the date the paper application was signed, or the date received for electronic");
    }
    
    void UIUserOrganizationDesignation(string uiUserOrganizationDesignation)
    {
        var regex = @"^\s{2}$";
        var match = Regex.Match(uiUserOrganizationDesignation, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid - should be blanks");
        }

    } 
    
    void OutOfAreaFlag(string outOfAreaFlag)
    {
        var regex = @"^\[NY/s]$";
        var match = Regex.Match(outOfAreaFlag, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Out of Area Flag");
        }
    }
    
    void SegmentNumber(string segmentNumber)
    {
        var regex = @"^\s{3}$";
        var match = Regex.Match(segmentNumber, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid - should be blanks");
        }

    } 
    
    void PartCBeneficiaryPremium(string partCBeneficiaryPremium)
    {
        var partCregex = @"^\s{7}$";
        var partCmatch = Regex.Match(partCBeneficiaryPremium, partCregex);
        if (!partCmatch.Success)
        {
            var regex = @"^\s\s\d\d\/.\d\d$";
            var match = Regex.Match(partCBeneficiaryPremium, regex);
            if (!match.Success)
            {
                Console.WriteLine(
                    "It doesn't work");
            }
        }

    } 
    void PartDBeneficiaryPremium(string partDBeneficiaryPremium)
    {
        var partDregex = @"^\s{7}$";
        var partDmatch = Regex.Match(partDBeneficiaryPremium, partDregex);
        if (!partDmatch.Success)
        {
           Console.WriteLine(
                    "Invalid - should be blanks")
           
        }
    } 
    void ElectionTypeCode(string electionTypeCode)
    {
        var regex = @"^\[ACDFIJLMNORSTUVWXYZ/s]$";
        var match = Regex.Match(electionTypeCode, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Election type code");
        }
    }
    
    void EnrollmentSourceCode(string enrollmentSourceCode)
    {
        var regex = @"^\[ABCDEFGHIJKLN/s]$";
        var match = Regex.Match(enrollmentSourceCode, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid enrollment Source Code");
        }
    }
    
    void PartDOptOutFlag(string partDOptOutFlag)
    {
        var regex = @"^\[YN/s]$";
        var match = Regex.Match(partDOptOutFlag, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Part D OptOut Flag");
        }
    }
    
    void PtsCAndDPremiumWithholdOpt(string ptsCAndDPremiumWithholdOpt)
    {
        var regex = @"^\[DNRS/s]$";
        var match = Regex.Match(ptsCAndDPremiumWithholdOpt, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Pts C And D Premium Withhold Opt - 120, 185, and 186 report the PPO involved in the communication, all other report the PPO in effect");
        }
    }
    
    void CumulativeNoOfUncoverdMonths(string cumulativeNoOfUncoverdMonths)
    {
        cumulativeNoOfUncoverdMonths = "000";
        
    } 
    void CreditableCoverageFlag(string creditableCoverageFlag)
    {
        var regex = @"^\[YNALRTU/s]$";
        var match = Regex.Match(creditableCoverageFlag, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Creditable Coverage Flag");
        }
    }
    void EmployerSubsidyOverrideFlag(string employerSubsidyOverrideFlag)
    {
        var regex = @"^\[Y/s]$";
        var match = Regex.Match(employerSubsidyOverrideFlag, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Employer Subsidy Override Flag");
        }
    }
    void ProcessingTimestamp(string processingTimestamp)
    {
        var regex = @"^\d\d/.\d\d/.\d\d/.\d\d\d\d\d$";
        var match = Regex.Match(processingTimestamp, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid timestamp");
        }
    }

    void EndDate(string endDate)
    {
        var dateRegex = @"^\d{4}(0[1-9]|1[012])(0[1-9]|[12][0-9]|3[01])$";
        var match = Regex.Match(endDate, dateRegex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid End Date");
        }
    }
    void SubmittedNoOfUncoveredMonths(string submittedNoOfUncoveredMonths)
    {
        var regex = @"^\d\d\d$";
        var match = Regex.Match(submittedNoOfUncoveredMonths, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid SubmittedNoOfUncoveredMonths");
        }
    }

    void SecondaryDrugInsuranceFlag(string secondaryDrugInsuranceFlag)
    {
        var regex = @"^\[YN/s]$";
        var match = Regex.Match(secondaryDrugInsuranceFlag, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Secondary Drug Insurance Flag");
        }
    }
    void SecondaryRxId(string secondaryRxId)
    {
        var regex = @"^/s{20}$";
        var match = Regex.Match(secondaryRxId, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Secondary Rx Id");
        }
    }
    void SecondaryRxGroup(string secondaryRxGroup)
    {
        var regex = @"^/s{15}$";
        var match = Regex.Match(secondaryRxGroup, regex);
        if (!match.Success)
        {
            Console.WriteLine(
                "Invalid Secondary Rx Group");
        }
    }
    

























}