using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenterWindow.Contracts.Services;
public interface IWindowEnumerationService
{
    IEnumerable<WindowModel> GetDesktopWindows();
}
